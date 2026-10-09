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
/// CoreLib, and waits for it as long as the scheduler works: while its timer
/// ticks and its ticks switch threads, the policy gives the new thread its
/// turn, however many quanta the threads it ranks first take before it.
/// The wait ends on the two failures above instead: no tick for
/// <see cref="NoTickTimeoutMs"/>, or <see cref="NoSwitchTicks"/> ticks
/// without a single switch. Neither is measured against the new thread's
/// own start: a policy may rank other threads first for several quanta (a
/// woken stride thread can sit two quanta behind the global pass), and
/// under emulation the counter the <see cref="Stopwatch"/> reads (the TSC
/// on x64, the generic timer's on ARM64) runs on host time, so a
/// descheduled virtual CPU sees milliseconds pass while it takes no tick.</para>
/// </summary>
internal static unsafe partial class KernelThread
{
    /// <summary>
    /// Ticks without a single context switch before the scheduler counts as
    /// not switching at all: ten 10 ms quanta, well past the few a policy may
    /// leave the creator running before it ranks another thread first.
    /// </summary>
    internal const uint NoSwitchTicks = 10;

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
    /// to begin as long as the scheduler ticks and switches.
    /// </summary>
    /// <param name="entry">The thread's body; the thread exits when it returns.</param>
    /// <returns>
    /// True once the thread began. False when the scheduler is not running,
    /// its timer went <see cref="NoTickTimeoutMs"/> without a tick, or it
    /// handled <see cref="NoSwitchTicks"/> ticks without switching any
    /// thread: that thread then never runs <paramref name="entry"/>. One that
    /// never ran is taken off the run queue at once; one that ran but was
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

        uint readiedAtTick;
        uint readiedAtSwitch;
        using (InternalCpu.DisableInterruptsScope())
        {
            SchedulerManager.CreateThread(started.CpuId, started);
            SchedulerManager.ReadyThread(started.CpuId, started);
            readiedAtTick = SchedulerManager.TickCount;
            readiedAtSwitch = SchedulerManager.SwitchCount;
        }

        // Spin rather than halt: when no interrupt comes, a halt never ends.
        // A switch away from this thread shows up as a later switch count
        // once it runs again, so only a scheduler that switches nothing at
        // all ends the wait on the tick side.
        long noTickTimeout = Stopwatch.Frequency / MillisecondsPerSecond * NoTickTimeoutMs;
        uint lastTick = readiedAtTick;
        long lastTickAt = Stopwatch.GetTimestamp();
        while (handshake.IsPending)
        {
            uint tick = SchedulerManager.TickCount;
            long now = Stopwatch.GetTimestamp();
            if (tick != lastTick)
            {
                if (tick - readiedAtTick >= NoSwitchTicks && SchedulerManager.SwitchCount == readiedAtSwitch)
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
