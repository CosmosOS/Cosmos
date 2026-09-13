using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Manages scheduler lifecycle and dispatches to current scheduler.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public static class SchedulerManager
{

    private static IScheduler? s_currentScheduler;
    private static PerCpuState[]? s_cpuStates;
    private static uint s_cpuCount;
    private static SpinLock s_globalLock;
    private static bool s_enabled;
    private static uint s_nextThreadId;

    // Global thread registry: tracks ALL live threads across all states
    // (Running, Ready, Blocked, Sleeping). Used by GC to scan all thread stacks.
    // Allocated once at init to avoid heap allocations during GC.
    private static SchedulerThread?[]? s_allThreads;
    private static int s_allThreadCount;

    // Cumulative TotalRuntime of exited non-idle threads. Live-thread runtime
    // disappears from s_allThreads on UnregisterThread, so we move it here to
    // keep GetBusyCpuTimeNs monotonic across thread lifecycle.
    private static ulong s_exitedNonIdleRuntimeNs;

    /// <summary>
    /// The interval the boot path asks the platform to arm the scheduler
    /// timer at, in nanoseconds (10 ms), and the slice the built-in Stride
    /// policy tunes itself against. It is a starting value, not a running
    /// fact: a driver can retune the timer afterwards, so read
    /// <see cref="TickPeriodNs"/> for what the kernel is actually ticking
    /// at. A policy is free to preempt on its own terms and need not treat
    /// either number as its slice.
    /// </summary>
    public const ulong DefaultQuantumNs = 10_000_000;

    /// <summary>
    /// Nanoseconds per millisecond, used to convert sleep timeouts to timestamp units.
    /// Public so other kernel components (e.g. timer drivers) can share the unit conversion.
    /// </summary>
    public const ulong NanosecondsPerMillisecond = 1_000_000UL;

    /// <summary>Timer ticks between debug-live snapshot refreshes (~100ms at 100Hz).</summary>
    private const uint SnapshotRefreshTickInterval = 10;

    /// <summary>Number of initial timer ticks that are always logged to serial.</summary>
    private const uint InitialTickLogCount = 10;

    /// <summary>After the initial ticks, log every Nth timer tick to avoid flooding serial output.</summary>
    private const uint TickLogInterval = 50;

    /// <summary>
    /// Whether scheduler support is compiled into this kernel
    /// (the <c>CosmosEnableScheduler</c> feature switch). Internal: the ring
    /// already publishes this fact as <c>KernelFeatures.Scheduler</c> and
    /// <c>SchedulerInfo.IsSupported</c>, so a policy author reads it there.
    /// </summary>
    internal static bool IsEnabled => CosmosFeatures.SchedulerEnabled;

    /// <summary>
    /// Whether the boot path has built the per-CPU state, which is what
    /// makes the rest of this class usable. Blocking primitives (and
    /// drivers built on them) must check this before touching
    /// <see cref="GetCpuState"/>: with the scheduler feature compiled out,
    /// or before its library initializer runs, there is exactly one
    /// execution context, so callers fall back to spin/polled paths
    /// instead of blocking.
    /// </summary>
    public static bool IsReady => IsEnabled && s_cpuStates != null;

    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Scheduler support is disabled. Set CosmosEnableScheduler=true in your csproj to enable it.");
        }
    }

    // ========== Initialization ==========

    internal static void Initialize(uint cpuCount)
    {
        ThrowIfDisabled();

        s_cpuCount = cpuCount;
        s_cpuStates = new PerCpuState[cpuCount];

        for (uint i = 0; i < cpuCount; i++)
        {
            s_cpuStates[i] = new PerCpuState { CpuId = i };
        }

        // Pre-allocate thread registry
        s_allThreads = new SchedulerThread?[SchedulerThread.MaxThreadCount];
        s_allThreadCount = 0;

        Cosmos.Kernel.Core.Runtime.DebugLiveSnapshot.Initialize();
        Cosmos.Kernel.Core.Runtime.DebugLiveGCSnapshot.Initialize();
        Cosmos.Kernel.Core.Runtime.DebugLiveMemorySnapshot.Initialize();
    }

    /// <summary>
    /// Installs a scheduling policy. Every live thread moves with it. The
    /// outgoing policy, if any, gets <see cref="IScheduler.OnThreadExit"/>
    /// for each thread it was managing and then
    /// <see cref="IScheduler.ShutdownCpu"/> on every CPU. The incoming policy
    /// gets <see cref="IScheduler.InitializeCpu"/> on every CPU, then
    /// <see cref="IScheduler.OnThreadCreate"/> for each live thread and
    /// <see cref="IScheduler.OnThreadReady"/> for those that were waiting in
    /// a run queue, so no thread is left behind in a run structure that no
    /// longer exists. The whole swap runs with interrupts masked, so no tick
    /// can land in the window where the incoming policy is installed but its
    /// per-CPU state does not exist yet, and every CPU reschedules on its
    /// next interrupt exit so the incoming policy picks from its own queue.
    /// </summary>
    /// <param name="scheduler">Scheduler to install.</param>
    public static void SetScheduler(IScheduler scheduler)
    {
        ThrowIfCpuStateNotInitialized();

        // IRQ-safe, not the plain pair: between the ShutdownCpu loop nulling
        // every per-CPU slot and the InitializeCpu loop refilling them, the
        // installed policy is already the one the timer dispatches to. A tick
        // in that window reaches its hooks with an empty slot, and the
        // outgoing thread can be dropped from a run structure that no longer
        // exists.
        using (s_globalLock.AcquireIrqSafe())
        {
            SchedulerThread?[]? threads = s_allThreads;
            IScheduler? outgoing = s_currentScheduler;

            if (outgoing is not null)
            {
                if (threads is not null)
                {
                    for (int i = 0; i < threads.Length; i++)
                    {
                        SchedulerThread? thread = threads[i];
                        if (thread is null || thread.State == SchedulerThreadState.Dead)
                        {
                            continue;
                        }

                        outgoing.OnThreadExit(s_cpuStates[thread.CpuId], thread);
                    }
                }

                for (uint i = 0; i < s_cpuCount; i++)
                {
                    outgoing.ShutdownCpu(s_cpuStates[i]);
                }
            }

            s_currentScheduler = scheduler;

            for (uint i = 0; i < s_cpuCount; i++)
            {
                scheduler.InitializeCpu(s_cpuStates[i]);
            }

            if (threads is not null)
            {
                // Same hand-over a thread gets at creation: a record first,
                // then a place in the run queue if it is waiting for the CPU.
                // A running thread keeps running and blocked or sleeping ones
                // come back through ReadyThread when they wake, so only the
                // Ready ones are queued here. Without this walk the threads
                // sitting in the outgoing policy's run queue would stay Ready
                // and never be picked again.
                for (int i = 0; i < threads.Length; i++)
                {
                    SchedulerThread? thread = threads[i];
                    if (thread is null || thread.State == SchedulerThreadState.Dead)
                    {
                        continue;
                    }

                    PerCpuState state = s_cpuStates[thread.CpuId];
                    scheduler.OnThreadCreate(state, thread);
                    if (thread.State == SchedulerThreadState.Ready)
                    {
                        scheduler.OnThreadReady(state, thread);
                    }
                }
            }

            for (uint i = 0; i < s_cpuCount; i++)
            {
                s_cpuStates[i]._needReschedule = true;
            }
        }
    }

    // ========== Accessors ==========

    /// <summary>
    /// The installed scheduler, or <see langword="null"/> before
    /// <see cref="SetScheduler"/> has run.
    /// </summary>
    public static IScheduler? Current => s_currentScheduler;

    /// <summary>
    /// Number of CPUs the scheduler manages.
    /// </summary>
    internal static uint CpuCount => s_cpuCount;

    /// <summary>
    /// Returns the scheduling state of a CPU, or <see langword="null"/>
    /// when <see cref="IsReady"/> is false.
    /// </summary>
    /// <param name="cpuId">
    /// CPU to look up. The count is on the ring as
    /// <c>SchedulerInfo.CpuCount</c>; a policy normally takes the state it
    /// needs from its hook parameters instead.
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="cpuId"/> is not a managed CPU and the scheduler is
    /// ready. Before it is ready this returns null for any value.
    /// </exception>
    public static PerCpuState? GetCpuState(uint cpuId) => s_cpuStates?[cpuId];

    /// <summary>
    /// The scheduling state of the CPU running this code, or
    /// <see langword="null"/> when <see cref="IsReady"/> is false. Internal:
    /// a policy is handed the <see cref="PerCpuState"/> it operates on by
    /// every hook, so it never has to ask which CPU it is on.
    /// </summary>
    internal static PerCpuState? CurrentCpuState => s_cpuStates?[GetCurrentCpuId()];

    /// <summary>
    /// Returns the per-CPU state array, or <see langword="null"/> before
    /// initialization.
    /// </summary>
    internal static PerCpuState[]? GetAllCpuStates() => s_cpuStates;

    /// <summary>
    /// Makes <paramref name="idleThread"/> the idle and current thread of a CPU
    /// and registers it. Boot calls this before the first policy is installed:
    /// from here on the CPU has a current thread, so thread statics resolve
    /// and a policy hook may run a class constructor. <see cref="SetScheduler"/>
    /// then hands the thread to the policy like any other live thread.
    /// </summary>
    internal static void SetupIdleThread(uint cpuId, SchedulerThread idleThread)
    {
        ThrowIfCpuStateNotInitialized();

        var state = s_cpuStates[cpuId];
        state.IdleThread = idleThread;
        state.CurrentThread = idleThread;
        RegisterThread(idleThread);
    }

    /// <summary>
    /// Whether the scheduler is processing timer ticks and preempting
    /// threads. The boot path arms it once the manager, the policy and the
    /// idle threads are all wired, so the first tick cannot race a
    /// half-built scheduler. Surfaced on the ring as
    /// <c>SchedulerInfo.IsRunning</c>.
    /// </summary>
    internal static bool IsRunning
    {
        get => s_enabled;
        set => s_enabled = value;
    }

    /// <summary>
    /// Allocates a new unique thread ID.
    /// </summary>
    internal static uint AllocateThreadId() => s_nextThreadId++;

    // ========== Thread Entry Dispatch ==========
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "StartThread")]
    private static extern void StartThread(SysThread aThis, IntPtr parameter);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "StopThread")]
    private static extern void StopThread(SysThread aThis, SysThread thread);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_stopped")]
    private static extern ref ManualResetEvent GetStoppedEvent(SysThread thread);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_startException")]
    private static extern ref Exception? GetStartException(SysThread thread);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_WaitInfo")]
    [return: UnsafeAccessorType("System.Threading.WaitSubsystem+ThreadWaitInfo, System.Private.CoreLib")]
    private static extern object GetWaitInfo(SysThread thread);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "OnThreadExiting")]
    private static extern void OnThreadExiting([UnsafeAccessorType("System.Threading.WaitSubsystem+ThreadWaitInfo, System.Private.CoreLib")] object waitInfo);

    /// <summary>
    /// Stop <paramref name="thread"/> on the managed side from another
    /// thread's context. Mirrors CoreLib's <c>Thread.OnThreadExit</c>, which
    /// cannot be pointed at a thread other than the one it runs on: abandon
    /// the mutexes the thread holds, mark it stopped, and release its
    /// joiners. A thread killed before <c>StartThread</c> ever ran on it is
    /// still unstarted on the managed side, so it takes the path CoreLib
    /// uses for a start that failed: its creator, still inside
    /// <c>Start()</c>, is released and throws a
    /// <see cref="ThreadStartException"/> carrying the reason.
    /// </summary>
    private static void StopManagedThread(SysThread thread)
    {
        if ((thread.ThreadState & global::System.Threading.ThreadState.Unstarted) != 0)
        {
            GetStartException(thread) = new ThreadStateException("The thread was killed before it started.");
            GetStoppedEvent(thread).Set();
            return;
        }

        OnThreadExiting(GetWaitInfo(thread));
        StopThread(null!, thread);
        GetStoppedEvent(thread).Set();
    }

    /// <summary>
    /// <para>
    /// Entry body for newly scheduled threads. Called from
    /// <see cref="Cosmos.Kernel.Core.Bridge.ThreadNative.EntryPointStub"/>,
    /// whose address is passed as the initial RIP / PC to the context-switch
    /// assembly by whoever creates the thread (e.g. ThreadPlug).
    /// </para>
    ///
    /// This method handles exceptions, marks the thread as exited, and halts. The scheduler will
    /// never re-pick a halted thread; the halt loop is a safety net in case
    /// the exit path ever races with a context switch.
    /// </summary>
    /// <param name="parameter">Generic parameter of the Thread Start, it is decoded based on the <see cref="SchedulerThreadFlags"/> set in the thread.</param>
    internal static void InvokeCurrentThreadStart(IntPtr parameter)
    {
        PerCpuState? cpuState = CurrentCpuState;
        SchedulerThread? currentThread = cpuState?.CurrentThread;

        if (currentThread == null)
        {
            Panic.Halt("No current thread in InvokeCurrentThreadStart");
        }

        uint threadId = currentThread.Id;
        Serial.WriteString("[SCHED] Running thread ");
        Serial.WriteNumber(threadId);
        Serial.WriteString("\n");

        int exitCode = 0;
        if (parameter != IntPtr.Zero)
        {
            try
            {
                Serial.WriteString("[SCHED] Invoking thread entry\n");

                // Evaluate flags, if SchedulerThreadFlags.Managed is set then this thread comes from a managed thread,
                // if not then we assume it's a gc handle holding a delegate.
                if ((currentThread.Flags & SchedulerThreadFlags.Managed) != 0)
                {
                    StartThread(null!, parameter);
                }
                else
                {
                    var handle = GCHandle<Action>.FromIntPtr(parameter);
                    Action start = handle.Target;
                    handle.Dispose();
                    start();
                }
                Serial.WriteString("[SCHED] Thread entry completed\n");
            }
            catch (Exception ex)
            {
                exitCode = 1;
                // Re-query thread ID — locals may be clobbered across the catch funclet.
                PerCpuState? exCpuState = CurrentCpuState;
                uint exThreadId = exCpuState?.CurrentThread?.Id ?? 0;
                Serial.WriteString("[SCHED] Thread ");
                Serial.WriteNumber(exThreadId);
                Serial.WriteString(" threw exception: ");
                Serial.WriteString(ex.Message);
                Serial.WriteString("\n");
            }
        }
        else
        {
            Serial.WriteString("[SCHED] No entry delegate on thread ");
            Serial.WriteNumber(threadId);
            Serial.WriteString("\n");
        }

        // Re-query current thread for exit — locals may be corrupted after the catch funclet.
        PerCpuState? exitCpuState = CurrentCpuState;
        SchedulerThread? exitThread = exitCpuState?.CurrentThread;
        uint exitThreadId = exitThread?.Id ?? 0;

        Serial.WriteString("[SCHED] Thread ");
        Serial.WriteNumber(exitThreadId);
        Serial.WriteString(" exiting with code ");
        Serial.WriteNumber((uint)exitCode);
        Serial.WriteString("\n");

        if (exitThread != null)
        {
            ExitThread(GetCurrentCpuId(), exitThread);
        }

        // Halt forever — scheduler should not pick this thread again.
        while (true)
        {
            InternalCpu.Halt();
        }
    }

    // ========== Thread Registry (for GC stack scanning) ==========

    /// <summary>
    /// Returns the thread registry array. Safe to call from GC (no allocations).
    /// </summary>
    internal static SchedulerThread?[]? Threads => s_allThreads;

    /// <summary>
    /// Returns the number of registered threads. Safe to call from GC.
    /// </summary>
    internal static int ThreadCount => s_allThreadCount;

    /// <summary>
    /// Returns the CPU ID currently executing this code path. Single-CPU
    /// today. Internal: a policy is handed the CPU it operates on by every
    /// hook, and a caller holding a <see cref="SchedulerThread"/> reads
    /// <see cref="SchedulerThread.CpuId"/>, which stays correct under SMP where this
    /// constant does not.
    /// TODO(SMP): replace with x86_64 GS-relative per-CPU storage or ARM64 MPIDR_EL1
    /// affinity read once application processors are brought online.
    /// </summary>
    internal static uint GetCurrentCpuId() => 0;

    /// <summary>
    /// Sum of TotalRuntime across all non-idle threads, in nanoseconds.
    /// One timer tick is charged to exactly one current thread per CPU, so this sum
    /// over a wall-clock window equals total busy CPU time for that window.
    /// The walk masks interrupts because the exiting half of the same figure is
    /// split across two stores: <see cref="UnregisterThread"/> folds the thread's
    /// runtime into the exited total and then empties its slot, both inside
    /// <see cref="ExitThread"/>'s own scope. An unmasked walk that read the exited
    /// total first and met that pair mid-scan would count the thread in neither
    /// term and report less than the previous call, which is the one thing
    /// <c>SchedulerInfo.BusyCpuTimeNs</c> promises never happens.
    /// </summary>
    internal static ulong GetBusyCpuTimeNs()
    {
        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            SchedulerThread?[]? threads = s_allThreads;
            if (threads is null)
            {
                return 0;
            }

            ulong sum = s_exitedNonIdleRuntimeNs;
            for (int i = 0; i < threads.Length; i++)
            {
                SchedulerThread? t = threads[i];
                if (t is null)
                {
                    continue;
                }
                if ((t.Flags & SchedulerThreadFlags.IdleThread) != 0)
                {
                    continue;
                }
                sum += t.TotalRuntime;
            }
            return sum;
        }
    }

    internal static nint OnThreadExitCallback
    {
        get;
        set
        {
            Serial.WriteString("[SCHED] Setting thread exit callback: ");
            Serial.WriteHexWithPrefix((ulong)value);
            Serial.WriteString("\n");
            field = value;
        }
    }

    /// <summary>
    /// Registers a thread in the global registry. Called during thread
    /// creation. Runs with interrupts masked: the scan for a free slot and
    /// the store into it have to be one step, or two creators racing (or one
    /// preempted between them) both pick the same slot, the second store
    /// wins, and the thread it displaced runs unregistered with a stack the
    /// GC never scans.
    /// </summary>
    internal static void RegisterThread(SchedulerThread thread)
    {
        if (s_allThreads == null)
        {
            return;
        }

        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            // Idempotent: SetScheduler hands every registry entry to the
            // incoming policy exactly once, so a thread must hold one slot.
            for (int i = 0; i < s_allThreads.Length; i++)
            {
                if (s_allThreads[i] == thread)
                {
                    return;
                }
            }

            for (int i = 0; i < s_allThreads.Length; i++)
            {
                if (s_allThreads[i] is null)
                {
                    s_allThreads[i] = thread;
                    s_allThreadCount++;
                    return;
                }
            }
        }

        Serial.WriteString("[SCHED] WARNING: Thread registry full, cannot register thread ");
        Serial.WriteNumber(thread.Id);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// Unregisters a thread from the global registry. Called during thread exit.
    /// </summary>
    internal static void UnregisterThread(SchedulerThread thread)
    {
        if (s_allThreads == null)
        {
            return;
        }

        for (int i = 0; i < s_allThreads.Length; i++)
        {
            if (s_allThreads[i] == thread)
            {
                if ((thread.Flags & SchedulerThreadFlags.IdleThread) == 0)
                {
                    s_exitedNonIdleRuntimeNs += thread.TotalRuntime;
                }
                s_allThreads[i] = null;
                s_allThreadCount--;
                return;
            }
        }
    }

    [MemberNotNull(nameof(s_cpuStates))]
    private static void ThrowIfCpuStateNotInitialized()
    {
        // The switch first: with the scheduler compiled out the per-CPU state
        // is never built, so every member guarded here would otherwise report
        // "not initialized" for a kernel whose only mistake is a csproj line.
        ThrowIfDisabled();

        if (s_cpuStates is null)
        {
            throw new InvalidOperationException("The scheduler has not been initialized; the boot path builds its per-CPU state.");
        }
    }

    [MemberNotNull(nameof(s_currentScheduler))]
    private static void ThrowIfSchedulerNotSet()
    {
        ThrowIfDisabled();

        if (s_currentScheduler is null)
        {
            throw new InvalidOperationException("No scheduling policy is installed; call SchedulerManager.SetScheduler first.");
        }
    }


    // ========== Thread Operations ==========

    internal static void CreateThread(uint cpuId, SchedulerThread thread)
    {
        ThrowIfDisabled();
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        Serial.WriteString("[SCHED] CreateThread: entering\n");
        RegisterThread(thread);
        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            var state = s_cpuStates[cpuId];
            s_currentScheduler.OnThreadCreate(state, thread);
        }
        Serial.WriteString("[SCHED] CreateThread: done\n");
    }

    internal static void ReadyThread(uint cpuId, SchedulerThread thread)
    {
        ThrowIfDisabled();
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            var state = s_cpuStates[cpuId];

            // Only set to Ready if not a new thread (Created).
            // New threads stay Created until they actually start running.
            // This allows ScheduleFromInterrupt to detect first-time execution.
            if (thread.State != SchedulerThreadState.Created)
            {
                thread.State = SchedulerThreadState.Ready;
            }

            s_currentScheduler.OnThreadReady(state, thread);

            // Ask the next hardware-IRQ exit to reschedule: when this wake
            // comes from an ISR (InterruptEvent.Signal), the woken thread
            // would otherwise sit in the run queue until the next timer tick.
            state._needReschedule = true;

            Serial.WriteString("[SCHED] Thread ");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString(" is now ready, RSP=");
            Serial.WriteHexWithPrefix((ulong)thread.StackPointer);
            Serial.WriteString("\n");
        }
    }

    internal static void BlockThread(uint cpuId, SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = s_cpuStates[cpuId];

            thread.State = SchedulerThreadState.Blocked;
            s_currentScheduler.OnThreadBlocked(state, thread);

            // Ask the next IRQ exit to switch away (same as ReadyThread): a
            // blocked current thread otherwise keeps re-entering its halt
            // loop until the quantum tick preempts it — or forever when the
            // periodic tick is not running.
            state._needReschedule = true;

            Serial.WriteString("[SCHED] BlockThread id=");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString("\n");
        }
    }

    internal static void ExitThread(uint cpuId, SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        // The managed side of the thread has to be stopped too: its Stopped
        // state, its joiners' event, and any mutex it held. CoreLib's exit
        // callback does all three, but it takes no argument and reads
        // t_currentThread, so it can only clean the thread it runs on. A
        // thread exiting itself gets that callback; a thread reaped from
        // someone else's context (SchedulerInfo.RequestKill on a queued
        // thread) gets the same three steps addressed at it explicitly.
        if (ReferenceEquals(GetCpuState(cpuId)?.CurrentThread, thread))
        {
            nint managedCallback = OnThreadExitCallback;
            if (managedCallback != IntPtr.Zero)
            {
                Serial.WriteString("[SCHED] ExitThread: managed exit callback for thread ");
                Serial.WriteNumber(thread.Id);
                Serial.WriteString("\n");
                unsafe
                {
                    delegate* unmanaged<void> callback = (delegate* unmanaged<void>)managedCallback;
                    callback();
                }
            }
        }
        else if (thread._managedThread.IsAllocated)
        {
            Serial.WriteString("[SCHED] ExitThread: stopping managed thread ");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString(" from another context\n");
            StopManagedThread(thread._managedThread.Target);
        }

        if (thread._managedThread.IsAllocated)
        {
            thread._managedThread.Dispose();
            thread._managedThread = default;
        }

        Serial.WriteString("[SCHED] ExitThread: entering DisableInterruptsScope for thread ");
        Serial.WriteNumber(thread.Id);
        Serial.WriteString("\n");

        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = s_cpuStates[cpuId];

            // Return TLAB and track unused bytes before unregistering
            if (GarbageCollector.IsEnabled)
            {
                unsafe
                {
                    ulong unused = (ulong)(thread._allocContext.AllocLimit - thread._allocContext.AllocPtr);
                    GarbageCollector.AddDeadThreadNonAllocBytes(unused);
                    GarbageCollector.ReturnAllocContext(ref thread._allocContext);
                }
            }

            thread.State = SchedulerThreadState.Dead;
            s_currentScheduler.OnThreadExit(state, thread);
            UnregisterThread(thread);
            Serial.WriteString("[SCHED] ExitThread: OnThreadExit done\n");
        }
    }

    internal static void YieldThread(uint cpuId, SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (CPU.InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = s_cpuStates[cpuId];

            s_currentScheduler.OnThreadYield(state, thread);
        }
    }

    /// <summary>
    /// Puts a thread to sleep with a timeout.
    /// The thread may be woken up either by the timeout expires or when signaled.
    /// </summary>
    /// <param name="cpuId">CPU ID of the thread.</param>
    /// <param name="thread">Thread to sleep.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": it sets a deadline of now, so the next tick wakes the thread.</param>
    internal static void Sleep(uint cpuId, SchedulerThread thread, uint timeoutMs)
    {
        MarkSleeping(cpuId, thread, timeoutMs);

        // Only park the CPU while still Sleeping: if a wake already landed between
        // scope-dispose and this point, halting would sleep past it.
        if (thread.State == SchedulerThreadState.Sleeping)
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>
    /// Marks a thread Sleeping with a wake deadline without halting — for callers that must
    /// make the state change atomic with their own IRQ-off section (ConditionVariable.WaitTimeout)
    /// and park afterwards under a state guard.
    /// </summary>
    /// <param name="cpuId">CPU ID of the thread.</param>
    /// <param name="thread">Thread to sleep.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": it sets a deadline of now, so the next tick wakes the thread.</param>
    internal static void MarkSleeping(uint cpuId, SchedulerThread thread, uint timeoutMs)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState cpuState = s_cpuStates[cpuId];

            ulong timestamp = GetTimestamp();
            // WakeupTime is compared against GetTimestamp() — Stopwatch ticks —
            // so the offset must be in ticks too. Adding nanoseconds stretched
            // timeouts 16x on ARM64 (62.5 MHz generic timer) and shrank them
            // on multi-GHz x64 TSCs.
            ulong ticksPerMs = (ulong)Stopwatch.Frequency / 1000;
            thread.WakeupTime = timestamp + timeoutMs * ticksPerMs;

            s_currentScheduler.OnThreadBlocked(cpuState, thread);
            thread.State = SchedulerThreadState.Sleeping;
        }
    }

    /// <summary>
    /// Puts the current thread to sleep with a timeout.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": the next tick wakes the thread.</param>
    internal static void Sleep(uint timeoutMs)
    {
        SchedulerThread? currentThread = CurrentCpuState?.CurrentThread;
        if (currentThread != null)
        {
            Sleep(currentThread.CpuId, currentThread, timeoutMs);
        }
    }

    // ========== Scheduling ==========

    internal static bool OnTick(uint cpuId, ulong elapsedNs)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        var state = s_cpuStates[cpuId];
        return s_currentScheduler.OnTick(state, state.CurrentThread, elapsedNs);
    }

    internal static void Schedule(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        var state = s_cpuStates[cpuId];
        state._lock.Acquire();

        var prev = state.CurrentThread;
        var next = s_currentScheduler.PickNext(state) ?? state.IdleThread;

        if (next == null)
        {
            state._lock.Release();
            return;
        }

        if (next != prev)
        {
            state.CurrentThread = next;
            next.State = SchedulerThreadState.Running;
            next.LastScheduledAt = GetTimestamp();

            state._lock.Release();
            DoContextSwitch(prev, next);
        }
        else
        {
            state._lock.Release();
        }
    }

    /// <summary>
    /// Changes a thread's priority through the installed scheduler
    /// (<see cref="IScheduler.SetPriority"/>). Interpretation is
    /// scheduler-specific.
    /// </summary>
    /// <param name="cpuId">CPU whose run structure the update rewrites.</param>
    /// <param name="thread">Thread to reprioritize.</param>
    /// <param name="priority">New priority value.</param>
    public static void SetPriority(uint cpuId, SchedulerThread thread, long priority)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        var state = s_cpuStates[cpuId];
        state._lock.Acquire();
        try
        {
            s_currentScheduler.SetPriority(state, thread, priority);
        }
        finally
        {
            state._lock.Release();
        }
    }

    /// <summary>
    /// Returns a thread's priority as reported by the installed scheduler
    /// (<see cref="IScheduler.GetPriority"/>).
    /// </summary>
    /// <param name="thread">Thread to query.</param>
    public static long GetPriority(SchedulerThread thread)
    {
        ThrowIfSchedulerNotSet();

        return s_currentScheduler.GetPriority(thread);
    }

    // ========== Load Balancing ==========

    /// <summary>
    /// Asks the installed scheduler to pick the best CPU for a new or
    /// migrating thread (<see cref="IScheduler.SelectCpu"/>).
    /// </summary>
    /// <param name="thread">Thread being placed.</param>
    /// <param name="currentCpu">CPU the thread is currently on.</param>
    internal static uint SelectCpu(SchedulerThread thread, uint currentCpu)
    {
        ThrowIfSchedulerNotSet();

        return s_currentScheduler.SelectCpu(thread, currentCpu, s_cpuCount);
    }

    /// <summary>
    /// Masks maskable interrupts on the current CPU until the returned
    /// scope is disposed. Schedulers use this around the entry points the
    /// manager does not guard: <see cref="IScheduler.SetPriority"/>,
    /// <see cref="IScheduler.GetPriority"/>, the two run-queue diagnostics
    /// hooks, and any tuning setters the policy exposes of its own. The
    /// spinlock <see cref="SetPriority"/> holds is not a substitute: it
    /// excludes another caller, and the tick path takes no lock at all. The
    /// tick hooks, the thread-lifecycle hooks and the two per-CPU lifecycle
    /// hooks are already called with interrupts masked.
    /// </summary>
    public static InterruptMaskScope MaskInterrupts() => new(InternalCpu.DisableInterruptsScope());

    /// <summary>
    /// Gives the installed scheduler a load-balancing opportunity for one
    /// CPU (<see cref="IScheduler.Balance"/>).
    /// </summary>
    /// <param name="cpuId">CPU to balance.</param>
    internal static void Balance(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        var state = s_cpuStates[cpuId];
        s_currentScheduler.Balance(state, s_cpuStates);
    }

    // ========== Timer Interrupt Handling ==========

    // Debug counter to avoid flooding serial output
    private static uint s_tickCount;

    private static ulong s_tickPeriodNs;

    /// <summary>
    /// Interval between scheduler ticks in nanoseconds, as the timer last
    /// reported it, or 0 before the first tick. This is the real preemption
    /// granularity: whatever slice a policy believes it is handing out, it
    /// cannot preempt more finely than the timer fires. Surfaced on the ring
    /// as <c>SchedulerInfo.TickPeriodNs</c>.
    /// </summary>
    internal static ulong TickPeriodNs => s_tickPeriodNs;

    /// <summary>
    /// Called from timer interrupt handler to process scheduling.
    /// This is the main entry point for preemptive scheduling.
    /// </summary>
    /// <param name="cpuId">Current CPU ID.</param>
    /// <param name="currentRsp">Current RSP from IRQ context (pointer to saved context).</param>
    /// <param name="elapsedNs">Nanoseconds since last tick.</param>
    internal static void OnTimerInterrupt(uint cpuId, nuint currentRsp, ulong elapsedNs)
    {
        s_tickCount++;

        // Recorded before the early returns below, so the period is readable
        // whether or not the scheduler is armed yet. The timer owns this
        // number: the boot path asks for DefaultQuantumNs, but a driver can
        // retune the device afterwards, and on ARM64 the scheduler tick and
        // the ring's TimerManager are the same device.
        s_tickPeriodNs = elapsedNs;

        // Refresh the debug-live snapshot every 10 ticks (~100ms at 100Hz)
        // so the host-side QMP poller sees fresh thread state without
        // pausing the kernel.
        if ((s_tickCount % SnapshotRefreshTickInterval) == 0)
        {
            Cosmos.Kernel.Core.Runtime.DebugLiveSnapshot.Update();
            Cosmos.Kernel.Core.Runtime.DebugLiveGCSnapshot.Update();
            Cosmos.Kernel.Core.Runtime.DebugLiveMemorySnapshot.Update();
        }

        // Log first 10 ticks and then every 50 ticks
        if (s_tickCount <= InitialTickLogCount || s_tickCount % TickLogInterval == 0)
        {
            Serial.WriteString("[SCHED] Tick ");
            Serial.WriteNumber(s_tickCount);
            Serial.WriteString(" enabled=");
            Serial.WriteString(s_enabled ? "1" : "0");
            Serial.WriteString("\n");
        }

        if (!s_enabled || s_currentScheduler == null || s_cpuStates == null)
        {
            return;
        }

        if (cpuId >= s_cpuCount)
        {
            return;
        }

        var state = s_cpuStates[cpuId];
        if (state.CurrentThread == null)
        {
            return;
        }

        // Check and wake up sleeping threads whose timeout has expired
        CheckSleepingThreads(elapsedNs);

        // Update timing and check if preemption needed
        bool needsReschedule = s_currentScheduler.OnTick(state, state.CurrentThread, elapsedNs);

        if (needsReschedule)
        {
            ScheduleFromInterrupt(cpuId, currentRsp);
        }
    }

    /// <summary>
    /// Checks all sleeping threads and wakes those whose wakeup time has expired.
    /// Called from timer interrupt handler to implement timed waits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckSleepingThreads(ulong elapsedNs)
    {
        if (s_allThreads == null)
        {
            return;
        }

        ulong currentTime = GetTimestamp();

        for (int i = 0; i < s_allThreads.Length; i++)
        {
            SchedulerThread? thread = s_allThreads[i];
            if (thread is null || thread.State != SchedulerThreadState.Sleeping)
            {
                continue;
            }

            // Check if wakeup time has been reached
            if (currentTime >= thread.WakeupTime)
            {
                Serial.WriteString("[SCHED] Waking sleeping thread ");
                Serial.WriteNumber(thread.Id);
                Serial.WriteString(" (time expired)\n");

                // Wake the thread by marking it as ready
                thread.WakeupTime = 0;
                ReadyThread(thread.CpuId, thread);
            }
        }
    }

    /// <summary>
    /// Runs a pending reschedule request on hardware-IRQ exit. ReadyThread
    /// sets the request when it wakes a thread (typically an ISR-side
    /// <see cref="InterruptEvent.Signal"/>); device-IRQ exit doesn't
    /// otherwise reschedule — only the timer tick does — so a woken waiter
    /// would sit in the run queue for up to a full quantum. No-op when the
    /// timer path already staged a context switch for this interrupt: a
    /// second ScheduleFromInterrupt would save this frame's stack pointer
    /// into a thread whose real context lives elsewhere.
    /// </summary>
    /// <param name="cpuId">Current CPU ID.</param>
    /// <param name="currentRsp">Current RSP (pointer to saved context on stack).</param>
    internal static void ReschedulePendingFromIrq(uint cpuId, nuint currentRsp)
    {
        if (!s_enabled || s_currentScheduler == null || s_cpuStates == null || cpuId >= s_cpuCount)
        {
            return;
        }

        PerCpuState state = s_cpuStates[cpuId];
        if (!state._needReschedule)
        {
            return;
        }

        state._needReschedule = false;

        if (ContextSwitchNative.GetContextSwitchSp() != 0)
        {
            return;
        }

        ScheduleFromInterrupt(cpuId, currentRsp);
    }

    /// <summary>
    /// Performs scheduling from within an interrupt context.
    /// Picks next thread and sets up context switch if needed.
    /// </summary>
    /// <remarks>
    /// Nothing on this path may throw. It runs on the interrupt stack with
    /// interrupts masked, where a throw has no handler to unwind to and the
    /// dispatcher would walk an IRQ frame the CFI unwinder cannot describe.
    /// The state the body needs is therefore checked and answered with a
    /// no-op, not with the <c>ThrowIf</c> guards the callable members use.
    /// Both callers already establish these conditions, so a check that
    /// fails here is a bug elsewhere, not a caller error to report.
    /// </remarks>
    /// <param name="cpuId">Current CPU ID.</param>
    /// <param name="currentRsp">Current RSP (pointer to saved context on stack).</param>
    internal static void ScheduleFromInterrupt(uint cpuId, nuint currentRsp)
    {
        if (s_cpuStates is null || s_currentScheduler is null || cpuId >= s_cpuCount)
        {
            return;
        }

        PerCpuState state = s_cpuStates[cpuId];

        // No lock needed - interrupts are already disabled in interrupt context
        SchedulerThread? prev = state.CurrentThread;
        SchedulerThread? next = s_currentScheduler.PickNext(state) ?? state.IdleThread;

        if (next == null)
        {
            // No thread to switch to - just continue with current
            // This happens when all threads have exited
            return;
        }

        if (next != prev)
        {
            /*
            Serial.WriteString("[SCHED] Context switch: thread ");
            Serial.WriteNumber(prev?.Id ?? 0);
            Serial.WriteString(" -> ");
            Serial.WriteNumber(next.Id);
            Serial.WriteString(" RSP=");
            Serial.WriteHexWithPrefix((ulong)next.StackPointer);
            Serial.WriteString("\n");
            */

            // Save current thread's stack pointer
            if (prev != null)
            {
                prev.StackPointer = currentRsp;
                if (prev.State == SchedulerThreadState.Running)
                {
                    prev.State = SchedulerThreadState.Ready;
                }

                // Put previous thread back in run queue if still runnable
                if (prev.State == SchedulerThreadState.Ready)
                {
                    s_currentScheduler.OnThreadYield(state, prev);
                }
            }

            // Switch to next thread
            state.CurrentThread = next;

            // Check if this is a NEW thread (never run before) or RESUMED
            bool isNewThread = next.State == SchedulerThreadState.Created;

            next.State = SchedulerThreadState.Running;
            next.LastScheduledAt = GetTimestamp();

            // Request context switch - set new thread flag and target RSP
            ContextSwitchNative.SetContextSwitchNewThread(isNewThread ? 1 : 0);
            ContextSwitchNative.SetContextSwitchSp(next.StackPointer);
        }
    }

    // ========== Platform-specific ==========

    private static void DoContextSwitch(SchedulerThread? prev, SchedulerThread? next)
    {
        // This is for non-interrupt context switches (e.g., voluntary yield)
        // Not fully implemented - use ScheduleFromInterrupt for preemptive switching
        if (next == null)
        {
            return;
        }

        prev?.State = SchedulerThreadState.Ready;

        next.State = SchedulerThreadState.Running;
        ContextSwitchNative.SetContextSwitchSp(next.StackPointer);
    }

    private static ulong GetTimestamp()
    {
        return (ulong)Stopwatch.GetTimestamp();
    }
}
