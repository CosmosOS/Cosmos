using System.Diagnostics;

namespace Cosmos.Kernel.Core.Scheduler.Stride;

/// <summary>
/// Stride scheduler with interactive process support.
/// </summary>
internal class StrideScheduler : IScheduler
{
    public string Name => "Stride";

    /// <summary>
    /// Large constant for stride precision.
    /// </summary>
    public const ulong Stride1 = 1 << 20;

    /// <summary>
    /// Default tickets for new threads.
    /// </summary>
    public const ulong DefaultTickets = 100;

    /// <summary>
    /// Sleep 2x more than run = interactive.
    /// </summary>
    private const ulong InteractiveSleepRatio = 2;

    /// <summary>
    /// Priority boost decays after 5ms.
    /// </summary>
    private const ulong WakeupBoostDecayNs = 5_000_000;

    /// <summary>
    /// Nanoseconds per second, for converting the Stopwatch frequency into
    /// the tick count of a quantum.
    /// </summary>
    private const ulong NanosecondsPerSecond = 1_000_000_000;

    /// <summary>
    /// Largest catch-up, in quanta, a single global-pass update may account
    /// for. Bounds both the virtual-time jump and the multiply in
    /// <see cref="UpdateGlobalPass"/> when the timestamp delta is not a real
    /// elapsed time.
    /// </summary>
    private const ulong MaxCatchUpQuanta = 1000;

    /// <summary>
    /// Stopwatch ticks in one <see cref="SchedulerManager.DefaultQuantumNs"/>,
    /// captured by <see cref="InitializeCpu"/>. The tick path must not be the
    /// first reader of <see cref="Stopwatch.Frequency"/>: that read runs
    /// Stopwatch's class constructor through the runtime's class constructor
    /// runner, which takes a lock the interrupted thread may hold, and an
    /// interrupt handler cannot wait for it. Never zero once a CPU has been
    /// initialized.
    /// </summary>
    private static ulong s_ticksPerQuantum;

    // ========== Lifecycle ==========

    public void InitializeCpu(PerCpuState cpuState)
    {
        // Thread context (SetScheduler), so the first read of Stopwatch's
        // statics belongs here. The frequency it captures was calibrated by
        // the platform initializer, which runs before the scheduler is
        // installed; on ARM64 it is the generic timer's hardware constant.
        ulong ticksPerQuantum =
            (ulong)Stopwatch.Frequency * SchedulerManager.DefaultQuantumNs / NanosecondsPerSecond;
        s_ticksPerQuantum = ticksPerQuantum == 0 ? 1 : ticksPerQuantum;

        cpuState.SchedulerData = new StrideCpuData();
    }

    public void ShutdownCpu(PerCpuState cpuState)
    {
        cpuState.SchedulerData = null;
    }

    // ========== Thread Lifecycle ==========

    public void OnThreadCreate(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideThreadData data = new()
        {
            Tickets = DefaultTickets,
            Stride = Stride1 / DefaultTickets,
            Pass = 0,
            Remain = 0,
            // A thread inherited from another policy while blocked has no
            // wake of its own to date its sleep from. Anchoring it here keeps
            // that first wake from reading as an age-old sleep and earning an
            // interactive boost it never slept for.
            LastWakeup = GetTimestamp()
        };
        thread.SchedulerData = data;

        // A thread normally arrives Created and passes through OnThreadReady
        // before it runs; that is where a runnable thread's tickets enter the
        // total. A thread that is already running when it is handed over (the
        // boot thread at startup, or the caller of SetScheduler on a policy
        // swap) never takes that path, yet the OnThreadBlocked or
        // OnThreadExit that eventually follows subtracts its tickets. Count
        // them here so the total cannot underflow.
        if (thread.State == SchedulerThreadState.Running)
        {
            StrideCpuData? cpuData = CpuDataOf(cpuState);
            if (cpuData is not null)
            {
                UpdateGlobalPass(cpuData);
                data.Pass = (long)cpuData.GlobalPass;
                cpuData.TotalTickets += data.Tickets;
            }
        }
    }

    public void OnThreadReady(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        StrideThreadData? threadData = ThreadDataOf(thread);

        if (cpuData is null || threadData is null)
        {
            return;
        }

        UpdateGlobalPass(cpuData);

        ulong now = GetTimestamp();
        bool wasBlocked = thread.State == SchedulerThreadState.Blocked;

        if (wasBlocked)
        {
            ulong sleepDuration = now - threadData.LastWakeup;

            // Detect interactive behavior
            if (sleepDuration > 0 && thread.TotalRuntime > 0)
            {
                if (sleepDuration > thread.TotalRuntime * InteractiveSleepRatio)
                {
                    threadData.IsInteractive = true;
                }
            }

            // Apply priority boost for interactive threads
            if (threadData.IsInteractive)
            {
                threadData.Pass = (long)cpuData.GlobalPass - (long)(threadData.Stride / 2);
                threadData.IsBoosted = true;
            }
            else
            {
                // CFS-style cap to prevent starvation
                long minPass = (long)cpuData.GlobalPass - (long)(Stride1 * 2);
                long newPass = (long)cpuData.GlobalPass + threadData.Remain;
                threadData.Pass = Math.Max(newPass, minPass);
            }

            threadData.LastWakeup = now;
        }
        else
        {
            // New thread - start at global pass
            threadData.Pass = (long)cpuData.GlobalPass;
        }

        InsertByPass(cpuData, thread);
        cpuData.TotalTickets += threadData.Tickets;
    }

    public void OnThreadBlocked(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        StrideThreadData? threadData = ThreadDataOf(thread);

        if (cpuData is null || threadData is null)
        {
            return;
        }

        UpdateGlobalPass(cpuData);

        threadData.Remain = threadData.Pass - (long)cpuData.GlobalPass;
        threadData.SleepCount++;

        RemoveThreadFromQueue(cpuData.RunQueue, thread);
        cpuData.TotalTickets -= threadData.Tickets;
    }

    public void OnThreadExit(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        if (cpuData is null)
        {
            return;
        }

        StrideThreadData? threadData = ThreadDataOf(thread);

        // Remove from run queue using ReferenceEquals + RemoveAt
        // Note: List<T>.Remove/Contains crash due to EqualityComparer<T>.Default requiring broken runtime helpers
        RemoveThreadFromQueue(cpuData.RunQueue, thread);

        if (threadData is not null)
        {
            cpuData.TotalTickets -= threadData.Tickets;
        }

        thread.SchedulerData = null;
    }

    public void OnThreadYield(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        StrideThreadData? threadData = ThreadDataOf(thread);

        if (cpuData is null || threadData is null)
        {
            return;
        }

        // Ensure thread's pass is at least GlobalPass to prevent starvation of newer threads.
        // Without this, a thread that started with Pass=0 (like the idle thread) would
        // perpetually have lower pass than threads added later with Pass=GlobalPass.
        if (threadData.Pass < (long)cpuData.GlobalPass)
        {
            threadData.Pass = (long)cpuData.GlobalPass;
        }

        InsertByPass(cpuData, thread);
    }

    // ========== Scheduling Decisions ==========

    public SchedulerThread? PickNext(PerCpuState cpuState)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);

        if (cpuData is null || cpuData.RunQueue.Count == 0)
        {
            return null;
        }

        var selected = cpuData.RunQueue[0];
        cpuData.RunQueue.RemoveAt(0);

        return selected;
    }

    public void OnPickFailed(PerCpuState cpuState, SchedulerThread thread)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        if (cpuData is null)
        {
            return;
        }

        InsertByPass(cpuData, thread);
    }

    // Debug counter for OnTick logging
    private static uint s_onTickLogCount;

    public bool OnTick(PerCpuState cpuState, SchedulerThread current, ulong elapsedNs)
    {
        s_onTickLogCount++;

        if (current is null)
        {
            return false;
        }

        StrideCpuData? cpuData = CpuDataOf(cpuState);
        if (cpuData is null)
        {
            return false;
        }

        StrideThreadData? threadData = ThreadDataOf(current);

        // Thread may have exited - its SchedulerData would be null
        if (threadData is null)
        {
            return cpuData.RunQueue.Count > 0;
        }

        current.TotalRuntime += elapsedNs;

        ulong quantum = SchedulerManager.DefaultQuantumNs;
        threadData.Pass += (long)((threadData.Stride * elapsedNs) / quantum);

        // Decay priority boost
        if (threadData.IsBoosted)
        {
            ulong timeSinceWake = GetTimestamp() - threadData.LastWakeup;
            if (timeSinceWake > WakeupBoostDecayNs)
            {
                threadData.IsBoosted = false;
            }
        }

        UpdateGlobalPass(cpuData);

        // Log every 100 ticks
        if (s_onTickLogCount % 100 == 0)
        {
            Cosmos.Kernel.Core.IO.Serial.WriteString("[STRIDE] OnTick: current=");
            Cosmos.Kernel.Core.IO.Serial.WriteNumber(current.Id);
            Cosmos.Kernel.Core.IO.Serial.WriteString(" runQ=");
            Cosmos.Kernel.Core.IO.Serial.WriteNumber((uint)cpuData.RunQueue.Count);
            Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
        }

        // Check for preemption
        if (cpuData.RunQueue.Count > 0)
        {
            StrideThreadData? nextData = ThreadDataOf(cpuData.RunQueue[0]);
            if (nextData is not null && nextData.Pass < threadData.Pass)
            {
                return true;
            }
        }

        return elapsedNs >= quantum;
    }

    // ========== Load Balancing ==========

    public uint SelectCpu(SchedulerThread thread, uint currentCpu, uint cpuCount)
    {
        if ((thread.Flags & SchedulerThreadFlags.Pinned) != 0)
        {
            return currentCpu;
        }

        uint best = currentCpu;
        ulong bestLoad = GetCpuLoad(currentCpu);

        for (uint cpu = 0; cpu < cpuCount; cpu++)
        {
            if (cpu == currentCpu)
            {
                continue;
            }

            ulong load = GetCpuLoad(cpu);
            if (load < bestLoad * 80 / 100)
            {
                best = cpu;
                bestLoad = load;
            }
        }

        return best;
    }

    public void OnThreadMigrate(SchedulerThread thread, PerCpuState fromState, PerCpuState toState)
    {
        StrideCpuData? fromData = CpuDataOf(fromState);
        StrideCpuData? toData = CpuDataOf(toState);
        StrideThreadData? threadData = ThreadDataOf(thread);

        if (fromData is null || toData is null || threadData is null)
        {
            return;
        }

        RemoveThreadFromQueue(fromData.RunQueue, thread);
        fromData.TotalTickets -= threadData.Tickets;

        threadData.Pass = (long)toData.GlobalPass + threadData.Remain;

        InsertByPass(toData, thread);
        toData.TotalTickets += threadData.Tickets;
    }

    public void Balance(PerCpuState cpuState, PerCpuState[] allCpuStates)
    {
        StrideCpuData? cpuData = CpuDataOf(cpuState);
        if (cpuData is null || cpuData.RunQueue.Count > 0)
        {
            return;
        }

        PerCpuState? busiest = null;
        int maxCount = 0;

        foreach (var state in allCpuStates)
        {
            if (state == cpuState)
            {
                continue;
            }

            StrideCpuData? data = CpuDataOf(state);
            if (data is not null && data.RunQueue.Count > maxCount)
            {
                maxCount = data.RunQueue.Count;
                busiest = state;
            }
        }

        if (busiest is null || maxCount <= 1)
        {
            return;
        }

        StrideCpuData? busiestData = CpuDataOf(busiest);
        if (busiestData is null || busiestData.RunQueue.Count == 0)
        {
            return;
        }

        var victim = busiestData.RunQueue[busiestData.RunQueue.Count - 1];

        if ((victim.Flags & SchedulerThreadFlags.Pinned) == 0)
        {
            OnThreadMigrate(victim, busiest, cpuState);
        }
    }

    // ========== Dynamic Reconfiguration ==========

    public void SetPriority(PerCpuState cpuState, SchedulerThread thread, long priority)
    {
        if (priority <= 0)
        {
            priority = 1;
        }

        // The manager holds only a spinlock here, which excludes another
        // caller but not the tick, and this rewrites the run queue.
        using (SchedulerManager.MaskInterrupts())
        {
            StrideCpuData? cpuData = CpuDataOf(cpuState);
            StrideThreadData? threadData = ThreadDataOf(thread);

            if (cpuData is null || threadData is null)
            {
                return;
            }

            UpdateGlobalPass(cpuData);

            ulong oldTickets = threadData.Tickets;
            ulong newTickets = (ulong)priority;
            ulong newStride = Stride1 / newTickets;

            long remain = threadData.Pass - (long)cpuData.GlobalPass;
            remain = (remain * (long)newStride) / (long)threadData.Stride;
            threadData.Pass = (long)cpuData.GlobalPass + remain;

            cpuData.TotalTickets = cpuData.TotalTickets - oldTickets + newTickets;
            threadData.Tickets = newTickets;
            threadData.Stride = newStride;

            if (thread.State == SchedulerThreadState.Ready)
            {
                RemoveThreadFromQueue(cpuData.RunQueue, thread);
                InsertByPass(cpuData, thread);
            }
        }
    }

    public long GetPriority(SchedulerThread thread)
    {
        // The tick can replace the thread's extension record between the read
        // of the slot and the read of the field it holds.
        using (SchedulerManager.MaskInterrupts())
        {
            StrideThreadData? data = ThreadDataOf(thread);
            return data is not null ? (long)data.Tickets : 0;
        }
    }

    // ========== Private Helpers ==========

    // Read the extension slots with 'as', never a cast. Stride is the policy
    // most exposed to a foreign record, because it is the only one that can
    // be installed a second time: reinstalling it over another policy hands
    // its hooks every thread that policy created, still carrying the other
    // policy's bookkeeping. A cast would throw on the first such thread from
    // inside the timer interrupt; 'as' degrades it to "absent", which every
    // hook below already handles.
    private static StrideCpuData? CpuDataOf(PerCpuState cpuState)
    {
        return cpuState.SchedulerData as StrideCpuData;
    }

    private static StrideThreadData? ThreadDataOf(SchedulerThread thread)
    {
        return thread.SchedulerData as StrideThreadData;
    }

    private void UpdateGlobalPass(StrideCpuData cpuData)
    {
        if (cpuData.TotalTickets == 0)
        {
            return;
        }

        ulong now = GetTimestamp();
        ulong elapsed = now - cpuData.LastPassUpdate;
        ulong globalStride = Stride1 / cpuData.TotalTickets;

        // The elapsed delta is in Stopwatch ticks, not nanoseconds (multi-GHz
        // TSC on x64, 62.5 MHz generic timer on ARM64), so the quantum divisor
        // must be in ticks too, the same conversion MarkSleeping needs for
        // WakeupTime. Dividing ticks by DefaultQuantumNs advanced GlobalPass
        // several times too fast on x64, and the OnThreadYield anti-starvation
        // floor then snapped every spinner up to GlobalPass, flattening ticket
        // ratios into equal shares. The divisor is captured in InitializeCpu;
        // see s_ticksPerQuantum for why it is not read here.
        ulong ticksPerQuantum = s_ticksPerQuantum;

        // Bound the catch-up. Two deltas are not a real elapsed time: the very
        // first update, where LastPassUpdate is still 0 and the delta is the
        // whole timestamp, and the multi-million-second CNTPCT_EL0 readings
        // QEMU TCG produces on ARM64 (the test runner clamps those for the
        // same reason). Either one both explodes GlobalPass (the yield floor
        // then snaps every thread up to it, flattening ticket ratios) and can
        // overflow the multiply below.
        ulong maxElapsed = ticksPerQuantum * MaxCatchUpQuanta;
        if (elapsed > maxElapsed)
        {
            elapsed = maxElapsed;
        }

        cpuData.GlobalPass += (globalStride * elapsed) / ticksPerQuantum;
        cpuData.LastPassUpdate = now;
    }

    private void InsertByPass(StrideCpuData cpuData, SchedulerThread thread)
    {
        StrideThreadData? threadData = ThreadDataOf(thread);
        if (threadData is null)
        {
            return;
        }

        int index = 0;

        for (; index < cpuData.RunQueue.Count; index++)
        {
            StrideThreadData? otherData = ThreadDataOf(cpuData.RunQueue[index]);
            if (otherData is null)
            {
                continue;
            }

            if (threadData.Pass <= otherData.Pass)
            {
                break;
            }
        }

        cpuData.RunQueue.Insert(index, thread);
    }

    private ulong GetCpuLoad(uint cpuId)
    {
        var state = SchedulerManager.GetCpuState(cpuId);
        StrideCpuData? data = state is not null ? CpuDataOf(state) : null;
        return data?.TotalTickets ?? 0;
    }

    private ulong GetTimestamp()
    {
        return (ulong)Stopwatch.GetTimestamp();
    }

    /// <summary>
    /// Removes a thread from the queue using ReferenceEquals + RemoveAt.
    /// TODO: List.Remove/Contains crash due to EqualityComparer requiring broken runtime helpers.
    /// </summary>
    private void RemoveThreadFromQueue(System.Collections.Generic.List<SchedulerThread> queue, SchedulerThread thread)
    {
        using (SchedulerManager.MaskInterrupts())
        {
            for (int i = 0; i < queue.Count; i++)
            {
                if (object.ReferenceEquals(queue[i], thread))
                {
                    queue.RemoveAt(i);
                    return;
                }
            }
        }
    }

    // ========== Diagnostics ==========

    public int GetRunQueueCount(PerCpuState cpuState)
    {
        // Disable interrupts to prevent timer from modifying RunQueue while we read
        using (SchedulerManager.MaskInterrupts())
        {
            StrideCpuData? cpuData = CpuDataOf(cpuState);
            return cpuData?.RunQueue.Count ?? 0;
        }
    }

    public SchedulerThread? GetRunQueueThread(PerCpuState cpuState, int index)
    {
        // Disable interrupts to prevent timer from modifying RunQueue while we read
        using (SchedulerManager.MaskInterrupts())
        {
            StrideCpuData? cpuData = CpuDataOf(cpuState);
            if (cpuData is null || index < 0 || index >= cpuData.RunQueue.Count)
            {
                return null;
            }

            return cpuData.RunQueue[index];
        }
    }
}
