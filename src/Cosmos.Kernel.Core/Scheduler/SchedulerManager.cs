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
/// <para>
/// Three locks keep the mechanism correct with several CPUs touching it at once (SMP phase 3a,
/// CosmosOS/Cosmos#3340). Each CPU's queue lock (<see cref="PerCpuState"/>) is held around
/// every policy hook that touches that CPU's run structure, around its current thread and
/// around its reschedule request; the owning CPU takes it from its tick and its interrupt
/// exits, a remote CPU takes it to wake a thread onto that CPU or to pull one off it. The
/// registry lock guards the thread registry and its counts. The heap lock is the collector's.
/// The order is queue locks in ascending CPU index, then the registry lock, then the heap
/// lock; two queue locks are held at once only by a migration. Nothing allocates while holding
/// any of them: a holder that started a collection would wait for the other CPUs to stop, and
/// a CPU spinning on its lock with interrupts masked cannot stop until it releases.
/// </para>
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public static class SchedulerManager
{

    private static IScheduler? s_currentScheduler;
    private static PerCpuState[]? s_cpuStates;
    private static uint s_cpuCount;
    private static bool s_enabled;
    private static uint s_nextThreadId;

    // The registry lock: around s_allThreads, s_allThreadCount and
    // s_exitedNonIdleRuntimeNs. Taken after a queue lock, never before one,
    // and never from the tick's sleeper walk, which reads the slots without it.
    private static SpinLock s_registryLock;

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

    /// <summary>Number of initial timer ticks logged to serial, to show at boot that the tick is live.</summary>
    private const uint InitialTickLogCount = 10;

    /// <summary>
    /// Whether scheduler support is compiled into this kernel
    /// (the <c>CosmosEnableScheduler</c> feature switch). Internal: the ring
    /// already publishes this fact as <c>KernelFeatures.Scheduler</c> and
    /// <c>SchedulerDiagnostics.IsSupported</c>, so a policy author reads it there.
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
    public static bool IsReady => IsEnabled && s_cpuStates is not null;

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

        Runtime.DebugLiveSnapshot.Initialize();
        Runtime.DebugLiveGCSnapshot.Initialize();
        Runtime.DebugLiveMemorySnapshot.Initialize();
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
    /// longer exists. The swap runs with the world stopped: interrupts masked
    /// on the calling CPU and every other online CPU held in its stop handler
    /// (<see cref="CpuStop"/>), so no tick, no wake and no interrupt exit on
    /// any CPU can reach a run structure while the incoming policy is
    /// installed but its per-CPU state does not exist yet. That is the whole
    /// exclusion: no spinlock is held, which is what lets the hooks allocate
    /// here, and every CPU reschedules on its next interrupt exit so the
    /// incoming policy picks from its own queue.
    /// </summary>
    /// <param name="scheduler">Scheduler to install.</param>
    public static void SetScheduler(IScheduler scheduler)
    {
        ThrowIfCpuStateNotInitialized();

        using (InternalCpu.DisableInterruptsScope())
        {
            CpuStop.StopOthers(s_cpuCount);

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
                Volatile.Write(ref s_cpuStates[i]._needReschedule, true);
            }

            // The release store that publishes the swap to the CPUs leaving
            // their stop handlers.
            CpuStop.ReleaseAll();
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
    /// <c>SchedulerDiagnostics.CpuCount</c>; a policy normally takes the state it
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
    /// Takes a CPU's queue lock, with interrupts masked on the calling CPU,
    /// until the returned scope is disposed. What a caller takes around a
    /// walk of that CPU's run structure through the diagnostics hooks
    /// (<see cref="IScheduler.GetRunQueueCount"/> then
    /// <see cref="IScheduler.GetRunQueueThread"/> for each index: each hook
    /// alone is atomic, the walk is not), or around a tuning setter a policy
    /// exposes of its own. The hooks themselves never take it: the mechanism
    /// holds it around every hook that touches the run structure, and the
    /// lock is not re-entrant, so a hook that took it would spin on itself.
    /// Nothing allocates under it.
    /// </summary>
    /// <param name="state">The CPU whose queue lock to hold.</param>
    public static CpuLockScope LockCpu(PerCpuState state)
    {
        return new(state._lock.AcquireIrqSafe());
    }

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

        PerCpuState state = s_cpuStates[cpuId];
        state.IdleThread = idleThread;
        state.CurrentThread = idleThread;
        if (!RegisterThread(idleThread))
        {
            Panic.Halt("The thread registry is full before the idle threads are registered.");
        }

        // The idle thread is the code running right now on this CPU, so from
        // here on a throw links into the thread's own chain, not the boot
        // code's. Only the running CPU's block can be the one: another CPU's
        // idle thread becomes current on that CPU when it comes online
        // (phase 2), through the same assignment.
        if (cpuId == GetCurrentCpuId())
        {
            PerCpuBlock.Current.CurrentExInfoHead = idleThread.ExInfoHeadSlot;
        }
    }

    /// <summary>
    /// Whether the scheduler is processing timer ticks and preempting
    /// threads. The boot path arms it once the manager, the policy and the
    /// idle threads are all wired, so the first tick cannot race a
    /// half-built scheduler. Surfaced on the ring as
    /// <c>SchedulerDiagnostics.IsRunning</c>. Volatile: written by the boot
    /// CPU, read by every CPU's tick.
    /// </summary>
    internal static bool IsRunning
    {
        get => Volatile.Read(ref s_enabled);
        set => Volatile.Write(ref s_enabled, value);
    }

    /// <summary>
    /// Allocates a new unique thread ID. Atomic: threads are created from any
    /// thread, and a bare increment read by two of them handed out one id twice.
    /// </summary>
    internal static uint AllocateThreadId() => Interlocked.Increment(ref s_nextThreadId) - 1;

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
    /// assembly by whoever creates the thread (e.g. SystemNative_CreateThread).
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

        if (currentThread is null)
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

        if (exitThread is not null)
        {
            ExitThread(exitThread);
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
    internal static int ThreadCount => Volatile.Read(ref s_allThreadCount);

    /// <summary>
    /// The index of the CPU executing this code path: one load from its
    /// per-CPU block (<see cref="PerCpuBlock"/>) through the per-CPU
    /// register, so the answer is this CPU's whatever CPU the caller runs on.
    /// Internal: a policy is handed the CPU it operates on by every hook, and
    /// a caller holding a <see cref="SchedulerThread"/> reads
    /// <see cref="SchedulerThread.CpuId"/>, the CPU the thread is assigned to.
    /// </summary>
    internal static uint GetCurrentCpuId() => PerCpuBlock.CurrentIndex;

    /// <summary>
    /// Sum of TotalRuntime across all non-idle threads, in nanoseconds.
    /// One timer tick is charged to exactly one current thread per CPU, so this sum
    /// over a wall-clock window equals total busy CPU time for that window.
    /// The walk holds the registry lock because the exiting half of the same figure is
    /// split across two stores: <see cref="UnregisterThread"/> folds the thread's
    /// runtime into the exited total and then empties its slot, under the same
    /// lock. A walk that read the exited total first and met that pair mid-scan would
    /// count the thread in neither term and report less than the previous call,
    /// which is the one thing <c>SchedulerDiagnostics.BusyCpuTimeNs</c> promises
    /// never happens.
    /// </summary>
    internal static ulong GetBusyCpuTimeNs()
    {
        using (s_registryLock.AcquireIrqSafe())
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
                if (t.IsIdle)
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
    /// Registers a thread in the global registry, under the registry lock:
    /// the scan for a free slot and the store into it have to be one step,
    /// or two creators racing both pick the same slot, the second store wins,
    /// and the thread it displaced runs unregistered with a stack the GC
    /// never scans. Fails closed on a full registry: the registry is the
    /// collector's root set, and a thread that ran unregistered would have
    /// the objects on its stack freed under it
    /// (valentinbreiz/nativeaot-patcher#444), so the creation seams refuse to
    /// start the thread instead.
    /// </summary>
    /// <returns>False when the registry is full and the thread was not registered.</returns>
    internal static bool RegisterThread(SchedulerThread thread)
    {
        if (s_allThreads is null)
        {
            return false;
        }

        using (s_registryLock.AcquireIrqSafe())
        {
            // Idempotent: SetScheduler hands every registry entry to the
            // incoming policy exactly once, so a thread must hold one slot.
            for (int i = 0; i < s_allThreads.Length; i++)
            {
                if (s_allThreads[i] == thread)
                {
                    return true;
                }
            }

            for (int i = 0; i < s_allThreads.Length; i++)
            {
                if (s_allThreads[i] is null)
                {
                    s_allThreads[i] = thread;
                    s_allThreadCount++;
                    return true;
                }
            }
        }

        Serial.WriteString("[SCHED] Thread registry full, thread ");
        Serial.WriteNumber(thread.Id);
        Serial.WriteString(" refused\n");
        return false;
    }

    /// <summary>
    /// Unregisters a thread from the global registry, under the registry lock. Called during
    /// thread exit.
    /// </summary>
    internal static void UnregisterThread(SchedulerThread thread)
    {
        if (s_allThreads is null)
        {
            return;
        }

        using (s_registryLock.AcquireIrqSafe())
        {
            for (int i = 0; i < s_allThreads.Length; i++)
            {
                if (s_allThreads[i] == thread)
                {
                    if (!thread.IsIdle)
                    {
                        s_exitedNonIdleRuntimeNs += thread.TotalRuntime;
                    }
                    s_allThreads[i] = null;
                    s_allThreadCount--;
                    return;
                }
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

    // ========== Queue locking ==========

    /// <summary>
    /// Takes the queue lock of the CPU <paramref name="thread"/> is assigned to and returns that
    /// CPU's state, the lock held. The CPU is read, the lock taken, and the CPU read again under
    /// it: a migration rewrites <see cref="SchedulerThread.CpuId"/> under both CPUs' locks, so a
    /// thread that moved between the first read and the lock is caught and the loop starts
    /// over on its new CPU. Interrupts masked by the caller; the caller releases.
    /// </summary>
    private static PerCpuState AcquireThreadQueueLock(SchedulerThread thread)
    {
        // Bounded by the migrations in flight: each pass that fails is one
        // migration that completed between the read and the lock.
        PerCpuState[] states = s_cpuStates!;
        uint cpuId = thread.CpuId;
        PerCpuState state = states[cpuId];
        state._lock.Acquire();
        while (thread.CpuId != cpuId)
        {
            state._lock.Release();
            cpuId = thread.CpuId;
            state = states[cpuId];
            state._lock.Acquire();
        }

        return state;
    }

    // ========== Thread Operations ==========

    /// <summary>
    /// Hands a new thread to the policy and registers it. The policy's record
    /// (<see cref="IScheduler.OnThreadCreate"/>) is made first, with interrupts
    /// masked but under no lock: the thread is reachable from no run structure
    /// and no registry slot yet, so nothing else can see it, and the hook may
    /// allocate the record there. Then the registry takes it, under the
    /// registry lock. A full registry refuses the thread
    /// (<see cref="RegisterThread"/>): the policy's record is dropped again
    /// and the caller must not start the thread.
    /// </summary>
    /// <param name="thread">The thread, <see cref="SchedulerThreadState.Created"/>, with its stack initialized.</param>
    /// <returns>False when the registry is full; the thread was not handed to the policy.</returns>
    internal static bool CreateThread(SchedulerThread thread)
    {
        ThrowIfDisabled();
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        Serial.WriteString("[SCHED] CreateThread: entering\n");
        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = s_cpuStates[thread.CpuId];
            s_currentScheduler.OnThreadCreate(state, thread);

            if (!RegisterThread(thread))
            {
                s_currentScheduler.OnThreadExit(state, thread);
                return false;
            }
        }
        Serial.WriteString("[SCHED] CreateThread: done\n");
        return true;
    }

    /// <summary>
    /// Makes a thread runnable on the CPU it is assigned to: under that CPU's
    /// queue lock the policy queues it (<see cref="IScheduler.OnThreadReady"/>),
    /// the thread turns <see cref="SchedulerThreadState.Ready"/> and the CPU's
    /// next interrupt exit is asked to reschedule. When that CPU is not the
    /// caller's, the reschedule interrupt is sent to it once the lock is
    /// released, so its exit runs the thread at once rather than on its next
    /// tick; a CPU halted in its idle loop wakes on it. Any context: thread
    /// context, the tick's sleeper walk, a device handler's signal.
    /// </summary>
    /// <param name="thread">The thread to wake, or to start for the first time.</param>
    internal static void ReadyThread(SchedulerThread thread)
    {
        ThrowIfDisabled();
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);

            // A wake for a thread that is already runnable does nothing: a
            // Ready thread is queued and a Running one is on its CPU, and
            // the run queue must never hold a thread twice (see
            // ScheduleFromInterruptLocked for what a second entry does).
            if (thread.State == SchedulerThreadState.Ready || thread.State == SchedulerThreadState.Running)
            {
                state._lock.Release();
                return;
            }

            // The hook runs before the state changes, so the policy sees what
            // the thread is waking from (Blocked, Sleeping, or Created for a
            // first start), the same order MarkSleeping keeps for
            // OnThreadBlocked. Under the lock, so nothing observes the thread
            // queued but not yet Ready.
            s_currentScheduler.OnThreadReady(state, thread);

            // Only set to Ready if not a new thread (Created).
            // New threads stay Created until they actually start running.
            // This allows ScheduleFromInterruptLocked to detect first-time execution.
            if (thread.State != SchedulerThreadState.Created)
            {
                thread.State = SchedulerThreadState.Ready;
            }

            // Ask the next hardware-IRQ exit to reschedule: when this wake
            // comes from an ISR (InterruptEvent.Signal), the woken thread
            // would otherwise sit in the run queue until the next timer tick.
            // No per-wake log here: this runs with interrupts masked, often
            // from an ISR, and each polled UART byte would stall every IRQ.
            Volatile.Write(ref state._needReschedule, true);

            uint cpuId = state.CpuId;
            state._lock.Release();

            // The interrupt after the lock, not under it: the target's exit
            // takes this lock first thing, and would only spin on it.
            if (cpuId != GetCurrentCpuId() && Volatile.Read(ref PerCpuBlock.At(cpuId).Online) != 0)
            {
                InterruptManager.SendReschedule(PerCpuBlock.At(cpuId).HardwareId);
            }
        }
    }

    /// <summary>
    /// Marks the calling thread <see cref="SchedulerThreadState.Blocked"/> and
    /// takes it out of its CPU's run structure
    /// (<see cref="IScheduler.OnThreadBlocked"/>), under the CPU's queue lock.
    /// The caller is the thread, on its own CPU, inside the primitive's
    /// IRQ-off section, and parks afterwards (<see cref="Park"/>).
    /// </summary>
    /// <param name="thread">The calling thread.</param>
    internal static void BlockThread(SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);

            thread.State = SchedulerThreadState.Blocked;
            s_currentScheduler.OnThreadBlocked(state, thread);

            // Ask the next IRQ exit to switch away (same as ReadyThread);
            // Park raises that interrupt at once. Without the flag, a
            // blocked current thread would wait for the quantum tick, or
            // forever when the periodic tick is not running. Not logged, for
            // the same reason as ReadyThread: interrupts are masked here.
            Volatile.Write(ref state._needReschedule, true);
            state._lock.Release();
        }
    }

    /// <summary>
    /// Ends a thread: stops its managed side, returns its allocation context
    /// to the collector, marks it <see cref="SchedulerThreadState.Dead"/> and
    /// removes it from its CPU's run structure (<see cref="IScheduler.OnThreadExit"/>)
    /// under the CPU's queue lock, then clears its registry slot under the
    /// registry lock, in that order. Called by the thread for itself at the
    /// end of its entry, by its creator for a thread that never ran, and by
    /// <c>SchedulerDiagnostics.RequestKill</c> for a thread waiting in a run
    /// queue.
    /// </summary>
    /// <param name="thread">The thread to end.</param>
    internal static void ExitThread(SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        // The managed side of the thread has to be stopped too: its Stopped
        // state, its joiners' event, and any mutex it held. CoreLib's exit
        // callback does all three, but it takes no argument and reads
        // t_currentThread, so it can only clean the thread it runs on. A
        // thread exiting itself gets that callback; a thread reaped from
        // someone else's context (SchedulerDiagnostics.RequestKill on a queued
        // thread) gets the same three steps addressed at it explicitly. Both
        // may allocate, so they run before any lock is taken.
        if (ReferenceEquals(CurrentCpuState?.CurrentThread, thread))
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

        using (InternalCpu.DisableInterruptsScope())
        {
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

            PerCpuState state = AcquireThreadQueueLock(thread);
            thread.State = SchedulerThreadState.Dead;
            s_currentScheduler.OnThreadExit(state, thread);
            state._lock.Release();

            // The registry lock after the queue lock: the order every path keeps.
            UnregisterThread(thread);
            Serial.WriteString("[SCHED] ExitThread: OnThreadExit done\n");
        }
    }

    /// <summary>
    /// Gives up the CPU at the next interrupt exit: sets the request
    /// <see cref="BlockThread"/> makes, so that exit switches even when the
    /// quantum has not run out. The switch re-queues the thread through
    /// <see cref="IScheduler.OnThreadYield"/> after the policy picked another,
    /// as a preemption does. Returns at once: nothing switches outside an
    /// interrupt exit, and waiting for one would cost every call up to a
    /// tick, while CoreLib's spin-then-block loops call Thread.Yield once per
    /// spin (70 times before an idle thread pool worker parks on one CPU).
    /// The request is this CPU's own and its exit consumes it under the queue
    /// lock; a bare store here is enough, since a store that lands after the
    /// exit's read is served by the next exit.
    /// </summary>
    /// <param name="cpuId">CPU ID of the calling thread.</param>
    internal static void YieldThread(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();

        Volatile.Write(ref s_cpuStates[cpuId]._needReschedule, true);
    }

    /// <summary>
    /// Puts a thread to sleep with a timeout.
    /// The thread may be woken up either by the timeout expires or when signaled.
    /// </summary>
    /// <param name="thread">The calling thread.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": it sets a deadline of now, so the next tick wakes the thread.</param>
    internal static void Sleep(SchedulerThread thread, uint timeoutMs)
    {
        MarkSleeping(thread, timeoutMs);
        Park(thread);
    }

    /// <summary>
    /// Takes the CPU from <paramref name="thread"/>, the caller, once it has
    /// marked itself Blocked (<see cref="BlockThread"/>) or Sleeping
    /// (<see cref="MarkSleeping"/>) and left its IRQ-off section, and
    /// returns once a wake has readied it and the scheduler has put it back
    /// on the CPU. The switch happens now: the controller raises its
    /// reschedule interrupt on this CPU and the exit of that interrupt runs
    /// the next thread. A halt would leave the CPU idle until the next tick,
    /// which every thread that blocks pays, up to a whole tick period each
    /// time, out of the time of the threads that could run. Returns at once
    /// when a wake already landed. The idle thread, and any thread before the
    /// controller can raise the interrupt, halts instead. Thread context,
    /// interrupts enabled.
    /// </summary>
    /// <param name="thread">The calling thread.</param>
    internal static void Park(SchedulerThread thread)
    {
        if (!IsParked(thread))
        {
            return;
        }

        PerCpuState[]? states = s_cpuStates;
        if (states is not null && !thread.IsIdle)
        {
            // This CPU's own request: the caller runs here, whatever CPU the
            // thread is assigned to once a wake moves it.
            Volatile.Write(ref states[GetCurrentCpuId()]._needReschedule, true);
            if (InterruptManager.TryRaiseReschedule())
            {
                // The interrupt lands within a few instructions and switches
                // away; this thread is back here once it runs again.
                while (IsParked(thread))
                {
                }

                return;
            }
        }

        // Only halt while still parked: if a wake already landed between
        // scope-dispose and this point, halting would sleep past it.
        if (IsParked(thread))
        {
            InternalCpu.Halt();
        }
    }

    /// <summary>
    /// Whether <paramref name="thread"/> is still Blocked or Sleeping. Not
    /// inlined, so <see cref="Park"/>'s wait reads the state on every pass:
    /// the interrupt that changes it is invisible to the compiler, and the
    /// wake may come from another CPU, which the volatile read of the state
    /// is what carries across.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsParked(SchedulerThread thread)
    {
        return thread.State is SchedulerThreadState.Blocked or SchedulerThreadState.Sleeping;
    }

    /// <summary>
    /// Marks a thread Sleeping with a wake deadline without halting — for callers that must
    /// make the state change atomic with their own IRQ-off section (ConditionVariable.WaitTimeout)
    /// and park afterwards under a state guard. Under the thread's CPU queue lock, as
    /// <see cref="BlockThread"/>.
    /// </summary>
    /// <param name="thread">The calling thread.</param>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": it sets a deadline of now, so the next tick wakes the thread.</param>
    internal static void MarkSleeping(SchedulerThread thread, uint timeoutMs)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState cpuState = AcquireThreadQueueLock(thread);

            ulong timestamp = GetTimestamp();
            // WakeupTime is compared against GetTimestamp() — Stopwatch ticks —
            // so the offset must be in ticks too. Adding nanoseconds stretched
            // timeouts 16x on ARM64 (62.5 MHz generic timer) and shrank them
            // on multi-GHz x64 TSCs.
            ulong ticksPerMs = (ulong)Stopwatch.Frequency / 1000;
            thread.WakeupTime = timestamp + timeoutMs * ticksPerMs;

            s_currentScheduler.OnThreadBlocked(cpuState, thread);
            thread.State = SchedulerThreadState.Sleeping;
            cpuState._lock.Release();
        }
    }

    /// <summary>
    /// Puts the current thread to sleep with a timeout.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds. 0 does not mean "forever": the next tick wakes the thread.</param>
    internal static void Sleep(uint timeoutMs)
    {
        SchedulerThread? currentThread = CurrentCpuState?.CurrentThread;
        if (currentThread is not null)
        {
            Sleep(currentThread, timeoutMs);
        }
    }

    /// <summary>
    /// Moves a thread that is not running to another CPU: under both CPUs'
    /// queue locks, taken in ascending index order, its <see cref="SchedulerThread.CpuId"/>
    /// is rewritten and, when it is waiting in a run structure, the policy
    /// moves its bookkeeping (<see cref="IScheduler.OnThreadMigrate"/>) and
    /// the destination is asked to reschedule, by interrupt when it is not
    /// the caller's CPU. A blocked or sleeping thread is in no run structure:
    /// only its CPU changes, and the wake that readies it queues it there.
    /// A running thread does not move here (its CPU's switch-out re-queue
    /// will, once affinity lands in phase 3b), nor an idle thread, which is
    /// its CPU's. Placement keeps every thread on CPU 0 until phase 5; this
    /// is the primitive the cells use to put one elsewhere.
    /// </summary>
    /// <param name="thread">The thread to move.</param>
    /// <param name="targetCpu">The CPU to move it to.</param>
    /// <returns>True once the thread is assigned to <paramref name="targetCpu"/>; false when it cannot move.</returns>
    internal static bool MigrateThread(SchedulerThread thread, uint targetCpu)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        if (targetCpu >= s_cpuCount || thread.IsIdle)
        {
            return false;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            // Both locks in index order, then the CPU read again under them,
            // as AcquireThreadQueueLock does with one. Bounded by the
            // migrations in flight on this thread.
            uint fromCpu = thread.CpuId;
            while (fromCpu != targetCpu)
            {
                PerCpuState first = s_cpuStates[Math.Min(fromCpu, targetCpu)];
                PerCpuState second = s_cpuStates[Math.Max(fromCpu, targetCpu)];
                first._lock.Acquire();
                second._lock.Acquire();

                if (thread.CpuId == fromCpu)
                {
                    PerCpuState fromState = s_cpuStates[fromCpu];
                    PerCpuState toState = s_cpuStates[targetCpu];
                    bool moved = MigrateLocked(thread, fromState, toState);

                    second._lock.Release();
                    first._lock.Release();

                    if (moved && targetCpu != GetCurrentCpuId() && thread.State == SchedulerThreadState.Ready
                        && Volatile.Read(ref PerCpuBlock.At(targetCpu).Online) != 0)
                    {
                        InterruptManager.SendReschedule(PerCpuBlock.At(targetCpu).HardwareId);
                    }

                    return moved;
                }

                second._lock.Release();
                first._lock.Release();
                fromCpu = thread.CpuId;
            }

            return true;
        }
    }

    /// <summary>The move itself, both queue locks held by the caller.</summary>
    private static bool MigrateLocked(SchedulerThread thread, PerCpuState fromState, PerCpuState toState)
    {
        switch (thread.State)
        {
            case SchedulerThreadState.Ready:
                thread.CpuId = toState.CpuId;
                s_currentScheduler!.OnThreadMigrate(thread, fromState, toState);
                Volatile.Write(ref toState._needReschedule, true);
                return true;

            case SchedulerThreadState.Blocked:
            case SchedulerThreadState.Sleeping:
                thread.CpuId = toState.CpuId;
                return true;

            default:
                // Running: on a CPU, whose switch-out is the only thing that
                // can take it off. Created: not yet handed to the policy, or
                // queued without the state saying so. Dead: gone.
                return false;
        }
    }

    // ========== Scheduling ==========

    internal static bool OnTick(uint cpuId, ulong elapsedNs)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        PerCpuState state = s_cpuStates[cpuId];
        using (state._lock.AcquireIrqSafe())
        {
            return s_currentScheduler.OnTick(state, state.CurrentThread, elapsedNs);
        }
    }

    internal static void Schedule(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        PerCpuState state = s_cpuStates[cpuId];
        using (state._lock.AcquireIrqSafe())
        {
            SchedulerThread? prev = state.CurrentThread;
            SchedulerThread? next = s_currentScheduler.PickNext(state) ?? state.IdleThread;

            if (next is null)
            {
                return;
            }

            if (next != prev)
            {
                state.CurrentThread = next;
                PerCpuBlock.Current.CurrentExInfoHead = next.ExInfoHeadSlot;
                next.State = SchedulerThreadState.Running;
                next.LastScheduledAt = GetTimestamp();

                DoContextSwitch(prev, next);
            }
        }
    }

    /// <summary>
    /// Changes a thread's priority through the installed scheduler
    /// (<see cref="IScheduler.SetPriority"/>), under the queue lock of the
    /// CPU the thread is assigned to, whose run structure the update
    /// rewrites. Interpretation is scheduler-specific.
    /// </summary>
    /// <param name="thread">Thread to reprioritize.</param>
    /// <param name="priority">New priority value.</param>
    public static void SetPriority(SchedulerThread thread, long priority)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);
            s_currentScheduler.SetPriority(state, thread, priority);
            state._lock.Release();
        }
    }

    /// <summary>
    /// Returns a thread's priority as reported by the installed scheduler
    /// (<see cref="IScheduler.GetPriority"/>), under the queue lock of the
    /// CPU the thread is assigned to.
    /// </summary>
    /// <param name="thread">Thread to query.</param>
    public static long GetPriority(SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);
            long priority = s_currentScheduler.GetPriority(thread);
            state._lock.Release();
            return priority;
        }
    }

    // ========== Load Balancing ==========

    /// <summary>
    /// Asks the installed scheduler to pick the best CPU for a new or
    /// migrating thread (<see cref="IScheduler.SelectCpu"/>). Dormant: nothing
    /// calls it until placement lands (SMP phase 5), and every thread is
    /// readied on the CPU it is assigned to.
    /// </summary>
    /// <param name="thread">Thread being placed.</param>
    /// <param name="currentCpu">CPU the thread is currently on.</param>
    internal static uint SelectCpu(SchedulerThread thread, uint currentCpu)
    {
        ThrowIfSchedulerNotSet();

        return s_currentScheduler.SelectCpu(thread, currentCpu, s_cpuCount);
    }

    /// <summary>
    /// Gives the installed scheduler a load-balancing opportunity for one
    /// CPU (<see cref="IScheduler.Balance"/>). Dormant, as <see cref="SelectCpu"/>.
    /// </summary>
    /// <param name="cpuId">CPU to balance.</param>
    internal static void Balance(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        PerCpuState state = s_cpuStates[cpuId];
        s_currentScheduler.Balance(state, s_cpuStates);
    }

    // ========== Timer Interrupt Handling ==========

    private static uint s_tickCount;

    private static uint s_switchCount;

    private static ulong s_tickPeriodNs;

    /// <summary>
    /// Timer interrupts CPU 0 handled since boot, counted before every early
    /// return of <see cref="OnTimerInterrupt"/>, so a tick that schedules
    /// nothing still counts. CPU 0's alone: every CPU's timer fires at the
    /// same period, and a count every CPU advanced would run N times faster
    /// than the clock the timed waits and the tick budgets are written
    /// against. Each CPU's own count is <see cref="PerCpuState.TickCount"/>.
    /// Wraps: compare differences. Any context.
    /// </summary>
    internal static uint TickCount => Volatile.Read(ref s_tickCount);

    /// <summary>
    /// Context switches staged since boot, by <see cref="ScheduleFromInterruptLocked"/>,
    /// the one path that switches threads. Wraps: compare differences. Any
    /// context.
    /// </summary>
    internal static uint SwitchCount => Volatile.Read(ref s_switchCount);

    /// <summary>
    /// Interval between scheduler ticks in nanoseconds, as the timer last
    /// reported it, or 0 before the first tick. This is the real preemption
    /// granularity: whatever slice a policy believes it is handing out, it
    /// cannot preempt more finely than the timer fires. Surfaced on the ring
    /// as <c>SchedulerDiagnostics.TickPeriodNs</c>.
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
        // Every CPU takes its own timer here (phase 2), on its own stack and
        // with its own saved context; the handler reads the state of the CPU
        // it runs on, under that CPU's queue lock, since a wake from another
        // CPU can be queuing a thread here at the same time. The pacing is
        // CPU 0's: the snapshot refresh, the boot log and the thread clock
        // follow its ticks, and every CPU counts its own in its state.
        if (cpuId == 0)
        {
            // Atomic: the counter is read from any thread.
            uint tick = Interlocked.Increment(ref s_tickCount);

            // Recorded before the early returns below, so the period is readable
            // whether or not the scheduler is armed yet. The timer owns this
            // number: the boot path asks for DefaultQuantumNs, but a driver can
            // retune the device afterwards, and on ARM64 the scheduler tick and
            // the ring's TimerManager are the same device.
            s_tickPeriodNs = elapsedNs;

            // Refresh the debug-live snapshot every 10 ticks (~100ms at 100Hz)
            // so the host-side QMP poller sees fresh thread state without
            // pausing the kernel.
            if ((tick % SnapshotRefreshTickInterval) == 0)
            {
                Runtime.DebugLiveSnapshot.Update();
                Runtime.DebugLiveGCSnapshot.Update();
                Runtime.DebugLiveMemorySnapshot.Update();
            }

            // Only the first ticks are logged, to show at boot that the tick is
            // live. This runs in the timer ISR with interrupts masked, and a
            // periodic line would stall every IRQ on polled UART bytes for the
            // whole uptime.
            if (tick <= InitialTickLogCount)
            {
                Serial.WriteString("[SCHED] Tick ");
                Serial.WriteNumber(tick);
                Serial.WriteString(" enabled=");
                Serial.WriteString(IsRunning ? "1" : "0");
                Serial.WriteString("\n");
            }
        }

        if (!IsRunning || s_currentScheduler is null || s_cpuStates is null)
        {
            return;
        }

        if (cpuId >= s_cpuCount)
        {
            return;
        }

        PerCpuState state = s_cpuStates[cpuId];
        Volatile.Write(ref state._tickCount, state._tickCount + 1);

        SchedulerThread? current = state.CurrentThread;
        if (current is null)
        {
            return;
        }

        // Check and wake up sleeping threads whose timeout has expired. CPU
        // 0's tick alone walks the registry; the wake of each sleeper goes
        // to the CPU the sleeper is assigned to, by interrupt when it is
        // another one. Before this CPU's lock is taken: a wake takes the
        // sleeper's CPU's lock, and two queue locks are held at once only by
        // a migration.
        if (cpuId == 0)
        {
            CheckSleepingThreads(elapsedNs);
        }

        // Interrupt context: interrupts are masked already, so the plain
        // acquire. A remote wake holds this lock for a few stores at most.
        state._lock.Acquire();

        // Update timing and check if preemption needed. The current thread is
        // this CPU's own, written by its switch under the lock just taken.
        bool needsReschedule = s_currentScheduler.OnTick(state, current, elapsedNs);

        if (needsReschedule)
        {
            ScheduleFromInterruptLocked(state, currentRsp);
        }

        state._lock.Release();
    }

    /// <summary>
    /// Checks all sleeping threads and wakes those whose wakeup time has expired.
    /// Called from timer interrupt handler to implement timed waits. The walk
    /// takes no lock: the registry lock orders the writers of the slots, and
    /// comes after a queue lock, which each wake below takes, so a walker
    /// holding it would take the locks in the wrong order. The array is never
    /// reallocated, a slot is one reference, and a thread that leaves its slot
    /// under the walk is Dead, which the state check skips.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckSleepingThreads(ulong elapsedNs)
    {
        if (s_allThreads is null)
        {
            return;
        }

        ulong currentTime = GetTimestamp();

        for (int i = 0; i < s_allThreads.Length; i++)
        {
            SchedulerThread? thread = Volatile.Read(ref s_allThreads[i]);
            if (thread is null || thread.State != SchedulerThreadState.Sleeping)
            {
                continue;
            }

            // Check if wakeup time has been reached
            if (currentTime >= thread.WakeupTime)
            {
                // Wake the thread by marking it as ready. Not logged, for the
                // same reason as ReadyThread: this is the timer ISR, with
                // interrupts masked.
                thread.WakeupTime = 0;
                ReadyThread(thread);
            }
        }
    }

    /// <summary>
    /// Runs a pending reschedule request on hardware-IRQ exit. ReadyThread
    /// sets the request when it wakes a thread (typically an ISR-side
    /// <see cref="InterruptEvent.Signal"/>, or a wake from another CPU, which
    /// also sends the interrupt whose exit lands here); device-IRQ exit doesn't
    /// otherwise reschedule — only the timer tick does — so a woken waiter
    /// would sit in the run queue for up to a full quantum. No-op when the
    /// timer path already staged a context switch for this interrupt: a
    /// second ScheduleFromInterrupt would save this frame's stack pointer
    /// into a thread whose real context lives elsewhere. The request is read
    /// and cleared under this CPU's queue lock, the lock the wake set it under.
    /// </summary>
    /// <param name="cpuId">Current CPU ID.</param>
    /// <param name="currentRsp">Current RSP (pointer to saved context on stack).</param>
    internal static void ReschedulePendingFromIrq(uint cpuId, nuint currentRsp)
    {
        if (!IsRunning || s_currentScheduler is null || s_cpuStates is null || cpuId >= s_cpuCount)
        {
            return;
        }

        PerCpuState state = s_cpuStates[cpuId];
        state._lock.Acquire();

        if (!Volatile.Read(ref state._needReschedule))
        {
            state._lock.Release();
            return;
        }

        Volatile.Write(ref state._needReschedule, false);

        if (ContextSwitchNative.GetContextSwitchSp() != 0)
        {
            state._lock.Release();
            return;
        }

        ScheduleFromInterruptLocked(state, currentRsp);
        state._lock.Release();
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
    /// fails here is a bug elsewhere, not a caller error to report. Takes the
    /// CPU's queue lock itself; the tick and the pending-reschedule exit, which
    /// hold it already, use the locked form.
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
        state._lock.Acquire();
        ScheduleFromInterruptLocked(state, currentRsp);
        state._lock.Release();
    }

    /// <summary>The switch itself, the CPU's queue lock held by the caller, on that CPU, in interrupt context.</summary>
    private static void ScheduleFromInterruptLocked(PerCpuState state, nuint currentRsp)
    {
        SchedulerThread? prev = state.CurrentThread;
        SchedulerThread? next = s_currentScheduler!.PickNext(state) ?? state.IdleThread;

        if (next is null)
        {
            // No thread to switch to - just continue with current
            // This happens when all threads have exited
            return;
        }

        if (next != prev)
        {
            Interlocked.Increment(ref s_switchCount);

            // A prev that is already Ready is queued: it parked itself
            // (blocked or slept) and a wake readied it before this switch
            // took it off the CPU. Yielding it again would leave a second
            // entry behind, whose pass keeps rising with the thread while its
            // place in the queue does not move, so the queue stops being
            // sorted and every thread queued behind that entry, a newly
            // created one included, never reaches the head.
            if (prev is not null)
            {
                prev.StackPointer = currentRsp;
                if (prev.State == SchedulerThreadState.Running)
                {
                    prev.State = SchedulerThreadState.Ready;
                    s_currentScheduler.OnThreadYield(state, prev);
                }
            }

            // Switch to next thread. The exception stubs find the running
            // thread's ExInfo chain through the per-CPU block: point it at the
            // next thread's before the switch, on this CPU's block, the one
            // the interrupt exit switches on.
            state.CurrentThread = next;
            PerCpuBlock.Current.CurrentExInfoHead = next.ExInfoHeadSlot;

            // Check if this is a NEW thread (never run before) or RESUMED
            bool isNewThread = next.State == SchedulerThreadState.Created;

            next.State = SchedulerThreadState.Running;
            next.LastScheduledAt = GetTimestamp();

            // Request context switch - set new thread flag and target RSP
            ContextSwitchNative.SetContextSwitchNewThread(isNewThread ? 1 : 0);
            ContextSwitchNative.SetContextSwitchSp(next.StackPointer);
        }
        else if (next.State == SchedulerThreadState.Ready)
        {
            // The policy handed back the current thread's own entry, queued
            // by a wake that came before it left the CPU: it stays on.
            next.State = SchedulerThreadState.Running;
        }
    }

    // ========== Platform-specific ==========

    private static void DoContextSwitch(SchedulerThread? prev, SchedulerThread? next)
    {
        // This is for non-interrupt context switches (e.g., voluntary yield)
        // Not fully implemented - use ScheduleFromInterrupt for preemptive switching
        if (next is null)
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
