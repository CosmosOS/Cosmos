using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Diagnostic view of the kernel scheduler: feature and lifecycle state,
/// per-CPU thread tables, and the global thread registry, plus a request
/// to terminate a thread by ID. All reads are allocation-free and safe to
/// poll from a monitor loop; snapshots of threads that are being
/// rescheduled concurrently may be one tick stale.
/// <para>
/// Three shapes report "nothing there", and which one a member uses follows
/// from what it is. A plain read answers with its own empty value, so
/// <see cref="CpuCount"/>, <see cref="ThreadCount"/>,
/// <see cref="ThreadSlotCount"/>, <see cref="TickPeriodNs"/>,
/// <see cref="BusyCpuTimeNs"/> and <see cref="GetRunQueueCount"/> are 0 and
/// <see cref="SchedulerName"/> is null before the scheduler exists, with no
/// separate error channel. A read that must hand back a whole snapshot cannot
/// express absence in the snapshot itself, so it is a <c>Try</c> and the bool
/// carries that answer. <see cref="RequestKill"/> is the one member that acts
/// rather than reads, and a bool would not tell a caller which of five things
/// happened, so it returns <see cref="ThreadKillResult"/>; that enum is its
/// failure channel as well, and it reports
/// <see cref="ThreadKillResult.NotFound"/> rather than throwing when the
/// scheduler is compiled out.
/// </para>
/// </summary>
public static class SchedulerInfo
{
    /// <summary>
    /// Nanoseconds per millisecond, for converting the nanosecond figures
    /// reported here into milliseconds.
    /// </summary>
    public const ulong NanosecondsPerMillisecond = 1_000_000;

    /// <summary>
    /// Whether scheduler support is compiled into this kernel
    /// (the <c>CosmosEnableScheduler</c> feature switch).
    /// </summary>
    public static bool IsSupported => CosmosFeatures.SchedulerEnabled;

    /// <summary>
    /// Whether a scheduler has been installed and per-CPU state exists.
    /// </summary>
    public static bool IsInitialized => SchedulerManager.Current is not null;

    /// <summary>
    /// Whether the scheduler is processing timer ticks and preempting
    /// threads.
    /// </summary>
    public static bool IsRunning => SchedulerManager.IsRunning;

    /// <summary>
    /// Name of the installed scheduler, or <see langword="null"/> before
    /// initialization.
    /// </summary>
    public static string? SchedulerName => SchedulerManager.Current?.Name;

    /// <summary>
    /// Number of CPUs the scheduler manages.
    /// </summary>
    public static uint CpuCount => SchedulerManager.CpuCount;

    /// <summary>
    /// Interval between scheduler ticks in nanoseconds, as the timer last
    /// reported it, or 0 before the first tick. This is the preemption
    /// granularity of the running kernel: whatever slice the installed
    /// policy believes it is handing out, it cannot preempt more finely than
    /// the timer fires. A policy's own slice is a policy-private number;
    /// nothing on the scheduler seam reports one.
    /// </summary>
    public static ulong TickPeriodNs => SchedulerManager.TickPeriodNs;

    /// <summary>
    /// Total busy CPU time in nanoseconds, summed over all CPUs and all
    /// non-idle threads since boot. Monotonic across thread exits; sample
    /// it over a wall-clock window to derive CPU utilization.
    /// </summary>
    public static ulong BusyCpuTimeNs => SchedulerManager.GetBusyCpuTimeNs();

    /// <summary>
    /// Number of live threads in the registry. This is a population count,
    /// not a bound: the registry is sparse, because a thread exiting leaves
    /// its slot empty while later slots stay occupied. Enumerate with
    /// <see cref="ThreadSlotCount"/> and <see cref="TryGetThreadInSlot"/>.
    /// </summary>
    public static int ThreadCount => SchedulerManager.ThreadCount;

    /// <summary>
    /// Number of slots in the thread registry, and the only valid bound for
    /// enumerating it. Slots can be empty; iterate from 0 to this value and
    /// probe each with <see cref="TryGetThreadInSlot"/>.
    /// </summary>
    public static int ThreadSlotCount => SchedulerManager.Threads?.Length ?? 0;

    /// <summary>
    /// Snapshots the thread in the given registry slot.
    /// </summary>
    /// <param name="slot">Registry slot index, from 0 to <see cref="ThreadSlotCount"/> exclusive.</param>
    /// <param name="info">Snapshot of the thread occupying the slot.</param>
    /// <returns><see langword="false"/> when the slot is out of range or empty.</returns>
    public static bool TryGetThreadInSlot(int slot, out KernelThreadInfo info)
    {
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (threads is null || slot < 0 || slot >= threads.Length || threads[slot] is not SchedulerThread thread)
        {
            info = default;
            return false;
        }

        info = Snapshot(thread);
        return true;
    }

    /// <summary>
    /// Snapshots the thread currently running on a CPU.
    /// </summary>
    /// <param name="cpuId">CPU to inspect.</param>
    /// <param name="info">Snapshot of the running thread.</param>
    /// <returns><see langword="false"/> when the CPU ID is out of range or no thread is current.</returns>
    public static bool TryGetCurrentThread(uint cpuId, out KernelThreadInfo info)
    {
        SchedulerThread? thread = cpuId < SchedulerManager.CpuCount
            ? SchedulerManager.GetCpuState(cpuId)?.CurrentThread
            : null;
        if (thread is null)
        {
            info = default;
            return false;
        }

        info = Snapshot(thread);
        return true;
    }

    /// <summary>
    /// Returns the number of threads waiting in a CPU's run queue.
    /// </summary>
    /// <param name="cpuId">CPU to inspect.</param>
    /// <returns>Queue length, or 0 when the CPU ID is out of range or the scheduler is not initialized.</returns>
    public static int GetRunQueueCount(uint cpuId)
    {
        IScheduler? scheduler = SchedulerManager.Current;
        PerCpuState? state = cpuId < SchedulerManager.CpuCount ? SchedulerManager.GetCpuState(cpuId) : null;
        return scheduler is not null && state is not null ? scheduler.GetRunQueueCount(state) : 0;
    }

    /// <summary>
    /// Snapshots a thread in a CPU's run queue by position.
    /// </summary>
    /// <param name="cpuId">CPU to inspect.</param>
    /// <param name="index">
    /// Queue position, from 0 to <see cref="GetRunQueueCount"/> exclusive.
    /// Each call is its own snapshot: a tick between the count and this read,
    /// or between two reads, reorders the queue, so a walk can see a thread
    /// twice or miss one. The count and the positions are for display, not
    /// for deciding anything.
    /// </param>
    /// <param name="info">Snapshot of the queued thread.</param>
    /// <returns><see langword="false"/> when the CPU ID or index is out of range.</returns>
    public static bool TryGetRunQueueThread(uint cpuId, int index, out KernelThreadInfo info)
    {
        IScheduler? scheduler = SchedulerManager.Current;
        PerCpuState? state = cpuId < SchedulerManager.CpuCount ? SchedulerManager.GetCpuState(cpuId) : null;
        SchedulerThread? thread = scheduler is not null && state is not null ? scheduler.GetRunQueueThread(state, index) : null;
        if (thread is null)
        {
            info = default;
            return false;
        }

        info = Snapshot(thread);
        return true;
    }

    /// <summary>
    /// Requests termination of a thread by ID. A thread waiting in a run
    /// queue is terminated immediately. A currently running thread is only
    /// marked dead: it stops being scheduled, but nothing reaps it, so it
    /// holds its registry slot and its stack for the life of the kernel.
    /// See <see cref="ThreadKillResult.MarkedForExit"/>.
    /// Idle threads are refused, and so are blocked or sleeping ones: they
    /// have already handed their share back to the policy, so they must be
    /// woken before they can be killed.
    /// </summary>
    /// <param name="threadId">ID of the thread to terminate.</param>
    /// <returns>What happened to the thread; see <see cref="ThreadKillResult"/>.</returns>
    public static ThreadKillResult RequestKill(uint threadId)
    {
        IScheduler? scheduler = SchedulerManager.Current;
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (scheduler is null || threads is null)
        {
            return ThreadKillResult.NotFound;
        }

        // Resolve against the registry rather than the run queues: a blocked
        // or sleeping thread is dequeued by OnThreadBlocked but stays
        // registered, and reporting it as NotFound would contradict the
        // snapshot TryGetThreadInSlot just handed the caller.
        SchedulerThread? target = null;
        for (int slot = 0; slot < threads.Length; slot++)
        {
            if (threads[slot] is SchedulerThread candidate && candidate.Id == threadId)
            {
                target = candidate;
                break;
            }
        }

        if (target is null)
        {
            return ThreadKillResult.NotFound;
        }

        if ((target.Flags & SchedulerThreadFlags.IdleThread) != 0)
        {
            return ThreadKillResult.RefusedIdle;
        }

        for (uint cpuId = 0; cpuId < SchedulerManager.CpuCount; cpuId++)
        {
            PerCpuState? state = SchedulerManager.GetCpuState(cpuId);
            if (state is null)
            {
                continue;
            }

            if (ReferenceEquals(state.CurrentThread, target))
            {
                target.State = SchedulerThreadState.Dead;
                return ThreadKillResult.MarkedForExit;
            }

            int count = scheduler.GetRunQueueCount(state);
            for (int i = 0; i < count; i++)
            {
                if (ReferenceEquals(scheduler.GetRunQueueThread(state, i), target))
                {
                    SchedulerManager.ExitThread(cpuId, target);
                    return ThreadKillResult.Killed;
                }
            }
        }

        return ThreadKillResult.RefusedBlocked;
    }

    private static KernelThreadInfo Snapshot(SchedulerThread thread)
    {
        bool hasPriority = thread.SchedulerData is not null;
        long priority = hasPriority ? SchedulerManager.Current?.GetPriority(thread) ?? 0 : 0;
        return new KernelThreadInfo(
            thread.Id,
            thread.CpuId,
            MapState(thread.State),
            (thread.Flags & SchedulerThreadFlags.IdleThread) != 0,
            (thread.Flags & SchedulerThreadFlags.Managed) != 0,
            thread.TotalRuntime,
            thread.StackSize,
            priority,
            hasPriority);
    }

    private static KernelThreadState MapState(SchedulerThreadState state) => state switch
    {
        SchedulerThreadState.Created => KernelThreadState.Created,
        SchedulerThreadState.Ready => KernelThreadState.Ready,
        SchedulerThreadState.Running => KernelThreadState.Running,
        SchedulerThreadState.Blocked => KernelThreadState.Blocked,
        SchedulerThreadState.Sleeping => KernelThreadState.Sleeping,
        _ => KernelThreadState.Dead,
    };
}
