// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Starts the kernel's own service threads (the USB and PCI Express
/// hot-plug threads) and bounds how long the caller waits for one to begin.
///
/// <para>CoreLib's <c>Thread.Start</c> does not return until the new thread
/// has run, and only a scheduler switch runs it. A timer that never ticks
/// (a LAPIC timer nothing calibrated), or one that ticks without
/// scheduling (a boot CPU whose APIC ID is not 0), keeps
/// that call spinning forever. <see cref="TryStart(Action)"/> builds the thread on
/// the scheduler directly, as <c>SystemNative_CreateThread</c> does for
/// CoreLib, and waits for it in scheduler ticks: the threads queued ahead
/// of it get a quantum each, then it does, so a scheduler that let
/// <see cref="StartGraceTicks"/> more ticks pass without running it does
/// not switch. Time is not the measure, since a switch only happens on a
/// tick, and under emulation the counter the <see cref="Stopwatch"/> reads
/// (the TSC on x64, the generic timer's on ARM64) runs on host time: a
/// descheduled virtual CPU sees milliseconds pass while it takes no tick.
/// The Stopwatch bounds only the stretch between two ticks,
/// <see cref="NoTickTimeoutMs"/>, which ends only for a timer that stopped.</para>
/// </summary>
internal static unsafe partial class KernelThread
{
    /// <summary>
    /// Ticks a new thread may wait beyond one per thread queued with it
    /// before the scheduler counts as not switching to it: the tick that
    /// preempts the creator, and one of margin.
    /// </summary>
    internal const uint StartGraceTicks = 2;

    /// <summary>
    /// Longest stretch without a scheduler tick, in milliseconds, before the
    /// timer counts as stopped and the wait ends: a hundred 10 ms quanta, so
    /// only a timer that never ticks pays it, not one an emulator delays.
    /// </summary>
    internal const uint NoTickTimeoutMs = 1000;

    private const long MillisecondsPerSecond = 1000;

#if ARCH_X64
    [LibraryImport("*", EntryPoint = "_native_x64_get_code_selector")]
    [SuppressGCTransition]
    private static partial ulong GetCurrentCodeSelector();
#endif

    /// <summary>
    /// Creates a thread that runs <paramref name="entry"/>, and waits for it
    /// to begin as long as the scheduler keeps ticking toward it.
    /// </summary>
    /// <param name="entry">The thread's body; the thread exits when it returns.</param>
    /// <returns>
    /// True once the thread began. False when the scheduler is not running,
    /// its timer stopped, or it handled one tick per queued thread and
    /// <see cref="StartGraceTicks"/> more without switching to the thread:
    /// that thread then never runs <paramref name="entry"/>. One that never
    /// ran is taken off the run queue at once; one that ran but was
    /// preempted before it looked at the handshake returns as soon as it
    /// resumes.
    /// </returns>
    internal static bool TryStart(Action entry) => TryStart(entry, out _);

    /// <summary>
    /// <see cref="TryStart(Action)"/> that also hands back the thread, for a
    /// caller that later needs to know whether it has exited.
    /// </summary>
    /// <param name="entry">The thread's body; the thread exits when it returns.</param>
    /// <param name="thread">The started thread, or null when the start failed.</param>
    /// <returns>True once the thread began; see <see cref="TryStart(Action)"/>.</returns>
    internal static bool TryStart(Action entry, [NotNullWhen(true)] out SchedulerThread? thread)
    {
        thread = null;
        if (!SchedulerManager.IsRunning)
        {
            return false;
        }

        StartHandshake handshake = new(entry);
        SchedulerThread started = new()
        {
            Id = SchedulerManager.AllocateThreadId(),
            CpuId = 0,
            State = SchedulerThreadState.Created,
        };

        // A thread without the Managed flag is started by
        // InvokeCurrentThreadStart as a call of the Action behind this
        // handle, which it frees first.
        GCHandle<Action> handle = new(handshake.Run);
        nuint parameter = (nuint)GCHandle<Action>.ToIntPtr(handle);
        nuint entryPoint = (nuint)(delegate* unmanaged<IntPtr, void>)&ThreadNative.EntryPointStub;
#if ARCH_ARM64
        // ARM64 has no code segment: the context ignores the selector.
        started.InitializeStack(entryPoint, 0, parameter);
#else
        started.InitializeStack(entryPoint, (ushort)GetCurrentCodeSelector(), parameter);
#endif

        uint tickBudget;
        uint readiedAtTick;
        using (InternalCpu.DisableInterruptsScope())
        {
            SchedulerManager.CreateThread(started.CpuId, started);
            SchedulerManager.ReadyThread(started.CpuId, started);

            // The queue holds the new thread too: with one quantum each, a
            // round-robin policy reaches it by the last of these ticks, and
            // the stride policy sooner, since a new thread enters at the
            // global pass.
            PerCpuState? cpuState = SchedulerManager.CurrentCpuState;
            int queued = cpuState is not null ? SchedulerManager.Current?.GetRunQueueCount(cpuState) ?? 0 : 0;
            tickBudget = (uint)queued + StartGraceTicks;
            readiedAtTick = SchedulerManager.TickCount;
        }

        // Spin rather than halt: when no interrupt comes, a halt never ends.
        long noTickTimeout = Stopwatch.Frequency / MillisecondsPerSecond * NoTickTimeoutMs;
        uint lastTick = readiedAtTick;
        long lastTickAt = Stopwatch.GetTimestamp();
        while (handshake.IsPending)
        {
            uint tick = SchedulerManager.TickCount;
            long now = Stopwatch.GetTimestamp();
            if (tick != lastTick)
            {
                if (tick - readiedAtTick >= tickBudget)
                {
                    break;
                }

                lastTick = tick;
                lastTickAt = now;
            }
            else if (now - lastTickAt >= noTickTimeout)
            {
                break;
            }
        }

        // Masked, so on this CPU, the only one that runs threads, the thread
        // cannot move between the verdict and the reap.
        using (InternalCpu.DisableInterruptsScope())
        {
            if (!handshake.TryAbandon())
            {
                // It began between the last look and the verdict.
                thread = started;
                return true;
            }

            // ReadyThread asked the next interrupt exit to reschedule, so a
            // device interrupt would still switch to a thread left queued. It
            // would exit at once, but then halt until a timer tick moves the
            // CPU on, and with a timer that never ticks the caller would not
            // run again. A thread that never ran holds nothing, so it goes
            // now, with the handle InvokeCurrentThreadStart would have freed.
            if (started.State == SchedulerThreadState.Created)
            {
                SchedulerManager.ExitThread(started.CpuId, started);
                handle.Dispose();
            }
        }

        return false;
    }

    /// <summary>
    /// The one word a new thread and its creator race on. The thread moves it
    /// from pending to started before it runs its entry, the creator from
    /// pending to abandoned when it stops waiting. Whichever compare-exchange
    /// lands first decides, so either the entry runs and the creator reports
    /// the thread started, or neither happens.
    /// </summary>
    private sealed class StartHandshake
    {
        private const int Pending = 0;
        private const int Started = 1;
        private const int Abandoned = 2;

        private readonly Action _entry;
        private int _state;

        /// <summary>True until the thread began or the creator gave up on it.</summary>
        public bool IsPending => Volatile.Read(ref _state) == Pending;

        public StartHandshake(Action entry) => _entry = entry;

        /// <summary>The new thread's body: runs the entry unless the creator gave up first.</summary>
        public void Run()
        {
            if (Interlocked.CompareExchange(ref _state, Started, Pending) == Pending)
            {
                _entry();
            }
        }

        /// <summary>Gives up on the thread; false when it began first.</summary>
        public bool TryAbandon() => Interlocked.CompareExchange(ref _state, Abandoned, Pending) == Pending;
    }
}
