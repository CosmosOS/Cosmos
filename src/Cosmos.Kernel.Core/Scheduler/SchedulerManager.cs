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

    // The set a thread created from an idle thread takes (SMP phase 3b):
    // CPU 0 until a kernel widens it through the ring. Read and written
    // under the registry lock, so a reader never sees half of a change.
    // Set in Initialize: a static initializer would give the manager a
    // class constructor, which runs through a lock that needs a current
    // thread, and the manager is reached from boot before one exists.
    private static CpuSet s_defaultAffinity;

    /// <summary>
    /// The CPUs the kernel's own service threads run on, whatever
    /// <see cref="DefaultAffinity"/> says: CPU 0, where the collector runs
    /// until it stops every CPU (SMP phase 4) and where the driver stack's
    /// interrupt masks exclude its handlers. Every thread
    /// <see cref="KernelThread.TryStart(Action)"/> starts takes it.
    /// </summary>
    internal static CpuSet HousekeepingCpus => CpuSet.Single(0);

    /// <summary>
    /// Bound (ms) of the self-move wait in <see cref="SetAffinity"/>: the
    /// reschedule interrupt the caller raised on its own CPU lands within a
    /// few instructions and the thread is back on its new CPU, so the bound
    /// only guards a set that changed back under the move and left the
    /// thread where it was.
    /// </summary>
    private const long SelfMoveTimeoutMs = 1000;

    /// <summary>
    /// Bound (ms) of <see cref="SetAffinity"/>'s wait on a parked thread
    /// whose CPU's stub has not left its stack yet: that stub runs within a
    /// few hundred instructions of the park, so the bound is only a guard.
    /// When it runs out the set is written and the thread's next switch-out
    /// evicts it.
    /// </summary>
    private const long OffCpuTimeoutMs = 1000;

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
        s_defaultAffinity = HousekeepingCpus;
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
    /// The thread running this code, or <see langword="null"/> before the
    /// scheduler has a current thread on this CPU. The boot thread, where
    /// <c>Main</c> runs, is CPU 0's idle thread: reachable this way and
    /// through no managed handle.
    /// </summary>
    internal static SchedulerThread? CurrentThread => CurrentCpuState?.CurrentThread;

    // ========== Affinity ==========

    /// <summary>
    /// The CPU set a thread created from an idle thread takes: CPU 0 until a
    /// kernel sets it. A thread created from any other thread takes its
    /// creator's set. Only the threads <c>System.Threading.Thread</c> starts
    /// look here; the kernel's own are <see cref="HousekeepingCpus"/>.
    /// </summary>
    internal static CpuSet DefaultAffinity
    {
        get
        {
            using (s_registryLock.AcquireIrqSafe())
            {
                return s_defaultAffinity;
            }
        }
    }

    /// <summary>
    /// Sets <see cref="DefaultAffinity"/>. False for an empty set or one that
    /// names no CPU the scheduler manages; the set may name CPUs that are not
    /// online, which the effective set drops.
    /// </summary>
    /// <param name="affinity">The set every new thread of an idle creator takes.</param>
    internal static bool TrySetDefaultAffinity(CpuSet affinity)
    {
        if (!NamesManagedCpu(affinity))
        {
            return false;
        }

        using (s_registryLock.AcquireIrqSafe())
        {
            s_defaultAffinity = affinity;
        }

        return true;
    }

    /// <summary>
    /// The CPUs that run their scheduler tick: CPU 0 and every application
    /// processor that came up (<see cref="PerCpuBlock.Online"/>). A snapshot;
    /// CPUs only come online in this tree, never go.
    /// </summary>
    internal static CpuSet OnlineCpus
    {
        get
        {
            CpuSet online = CpuSet.Single(0);
            for (uint cpu = 1; cpu < s_cpuCount; cpu++)
            {
                if (Volatile.Read(ref PerCpuBlock.At(cpu).Online) != 0)
                {
                    online = online.With(cpu);
                }
            }

            return online;
        }
    }

    /// <summary>
    /// The CPUs <paramref name="thread"/> may actually go to: its set
    /// restricted to the CPUs online, or every online CPU when that leaves
    /// nothing (a kernel pinned it to a CPU that is not up). Never empty.
    /// </summary>
    /// <param name="thread">The thread.</param>
    internal static CpuSet EffectiveAffinity(SchedulerThread thread)
    {
        return EffectiveOf(thread.Affinity);
    }

    private static CpuSet EffectiveOf(CpuSet affinity)
    {
        CpuSet online = OnlineCpus;
        CpuSet effective = affinity.Intersect(online);
        return effective.IsEmpty ? online : effective;
    }

    /// <summary>Whether the set names at least one CPU the scheduler manages.</summary>
    private static bool NamesManagedCpu(CpuSet affinity)
    {
        for (uint cpu = 0; cpu < s_cpuCount; cpu++)
        {
            if (affinity.Contains(cpu))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Finds the control block of a managed thread: a walk of the registry
    /// under its lock, comparing the handle each block keeps. Null for a
    /// thread the scheduler is not running (not started, exited) and for an
    /// idle thread, which no handle reaches; the caller resolves its own
    /// thread through <see cref="CurrentThread"/> first. Allocates nothing.
    /// </summary>
    /// <param name="managed">The managed thread.</param>
    internal static SchedulerThread? FindByManagedThread(SysThread managed)
    {
        if (s_allThreads is null)
        {
            return null;
        }

        using (s_registryLock.AcquireIrqSafe())
        {
            for (int i = 0; i < s_allThreads.Length; i++)
            {
                SchedulerThread? thread = s_allThreads[i];
                if (thread is not null && thread._managedThread.IsAllocated
                    && ReferenceEquals(thread._managedThread.Target, managed))
                {
                    return thread;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Changes the CPUs <paramref name="thread"/> may run on. Under the queue
    /// lock of the thread's CPU the set is written and the ideal CPU pulled
    /// inside it; when the effective set still allows that CPU nothing else
    /// happens. Otherwise a thread no CPU is on (parked, or queued) moves at
    /// once through <see cref="MigrateThread"/>; a thread a CPU is still on
    /// is left to that CPU's switch-out, which evicts a thread its CPU is
    /// excluded from: the CPU is asked to reschedule, by interrupt when it is
    /// not the caller's, and when the thread is the caller itself the call
    /// returns once it runs on its new CPU. A thread parked moments ago,
    /// whose CPU's stub has not left its stack, is waited for and then moved.
    /// Thread context, interrupts enabled; an idle thread never moves, and an
    /// empty set is refused.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="affinity">The CPUs it may run on.</param>
    /// <exception cref="ArgumentException"><paramref name="affinity"/> is empty.</exception>
    internal static void SetAffinity(SchedulerThread thread, CpuSet affinity)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        if (affinity.IsEmpty)
        {
            throw new ArgumentException("A thread's CPU set must name at least one CPU.", nameof(affinity));
        }

        if (thread.IsIdle)
        {
            return;
        }

        if (affinity.Intersect(OnlineCpus).IsEmpty)
        {
            // Thread context, no lock: the one place this is said.
            Serial.WriteString("[SCHED] Thread ");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString(" has no online CPU in its set, running it on any\n");
        }

        // One pass decides for a thread no CPU is on, or one that runs. A
        // thread parked an instant ago, whose CPU's stub has not left its
        // stack, is looked at again once it has, and so is a queued thread
        // its CPU picked between the verdict and the move. Bounded by that
        // stub, which nothing holds up.
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * OffCpuTimeoutMs / 1000;
        do
        {
            AffinityVerdict verdict;
            uint cpuId;
            using (InternalCpu.DisableInterruptsScope())
            {
                PerCpuState state = AcquireThreadQueueLock(thread);
                thread.Affinity = affinity;
                CpuSet effective = EffectiveOf(affinity);
                cpuId = state.CpuId;

                if (effective.Contains(cpuId))
                {
                    if (!effective.Contains(thread.IdealCpu))
                    {
                        thread.IdealCpu = cpuId;
                    }

                    verdict = AffinityVerdict.Stays;
                }
                else if (thread.State == SchedulerThreadState.Running)
                {
                    // On its CPU, or picked by it an instant ago: that CPU's
                    // next interrupt exit evicts it.
                    Volatile.Write(ref state._needReschedule, true);
                    verdict = cpuId == GetCurrentCpuId() ? AffinityVerdict.EvictSelf : AffinityVerdict.EvictRemote;
                }
                else if (!thread.IsOnCpu)
                {
                    // Parked or queued, no CPU on its stack: moved below,
                    // under both locks, once this one is released. Created
                    // or Dead: the first switch-out moves it, or nothing to.
                    verdict = thread.State is SchedulerThreadState.Ready
                        or SchedulerThreadState.Blocked or SchedulerThreadState.Sleeping
                        ? AffinityVerdict.MoveNow
                        : AffinityVerdict.Stays;
                }
                else
                {
                    // Parked, but its CPU's stub has not switched away yet.
                    verdict = AffinityVerdict.LookAgain;
                }

                state._lock.Release();
            }

            switch (verdict)
            {
                case AffinityVerdict.Stays:
                    return;

                case AffinityVerdict.MoveNow:
                    if (MigrateThread(thread, SelectCpuVerified(thread, EffectiveAffinity(thread))))
                    {
                        return;
                    }

                    // Its CPU picked it meanwhile: now Running, or on the stack.
                    break;

                case AffinityVerdict.EvictSelf:
                    SelfMove(thread, cpuId);
                    return;

                case AffinityVerdict.EvictRemote:
                    if (Volatile.Read(ref PerCpuBlock.At(cpuId).Online) != 0)
                    {
                        InterruptManager.SendReschedule(PerCpuBlock.At(cpuId).HardwareId);
                    }

                    return;
            }
        }
        while (Stopwatch.GetTimestamp() < deadline);
    }

    /// <summary>What one look at a thread under its queue lock told <see cref="SetAffinity"/>.</summary>
    private enum AffinityVerdict : byte
    {
        /// <summary>The effective set allows its CPU, or nothing can move it.</summary>
        Stays,

        /// <summary>Parked or queued with no CPU on its stack: <see cref="MigrateThread"/> now.</summary>
        MoveNow,

        /// <summary>The caller itself: raise the reschedule here and wait for the new CPU.</summary>
        EvictSelf,

        /// <summary>Running on another CPU: that CPU's next interrupt exit evicts it.</summary>
        EvictRemote,

        /// <summary>Parked, but its CPU's stub has not left its stack: look again.</summary>
        LookAgain,
    }

    /// <summary>
    /// The caller changed its own set to one that excludes its CPU: raises
    /// the reschedule interrupt here, as <see cref="Park"/> does, whose exit
    /// evicts and moves the thread, and spins until it runs on the new CPU,
    /// so the caller returns there. Falls back to the next tick when the
    /// interrupt cannot be raised yet.
    /// </summary>
    private static void SelfMove(SchedulerThread thread, uint cpuId)
    {
        if (!InterruptManager.TryRaiseReschedule())
        {
            return;
        }

        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * SelfMoveTimeoutMs / 1000;
        while (thread.CpuId == cpuId && Stopwatch.GetTimestamp() < deadline)
        {
        }
    }

    /// <summary>
    /// Sets the CPU the policy prefers for <paramref name="thread"/>. False
    /// when its set does not name <paramref name="cpu"/>; checked under the
    /// thread's queue lock, so a set change in flight is not raced.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="cpu">The CPU, one of the thread's set.</param>
    internal static bool SetIdealCpu(SchedulerThread thread, uint cpu)
    {
        ThrowIfCpuStateNotInitialized();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);
            bool allowed = cpu < s_cpuCount && thread.Affinity.Contains(cpu);
            if (allowed)
            {
                thread.IdealCpu = cpu;
            }

            state._lock.Release();
            return allowed;
        }
    }

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
            // And the idle thread is on this CPU: its first switch-out clears this.
            Volatile.Write(ref idleThread._onCpu, 1);
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

                    // The managed handle goes with the slot, under this lock:
                    // FindByManagedThread reads it under the same one.
                    if (thread._managedThread.IsAllocated)
                    {
                        thread._managedThread.Dispose();
                        thread._managedThread = default;
                    }

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
    /// <para>
    /// The thread's CPU set is assigned here (SMP phase 3b):
    /// <paramref name="affinity"/> when the caller names one (the kernel's
    /// own threads pass <see cref="HousekeepingCpus"/>), else the creating
    /// thread's set, or <see cref="DefaultAffinity"/> when the creator is an
    /// idle thread (the boot thread running <c>Main</c>). The ideal CPU is
    /// the creating CPU, and so is the thread's CPU when the effective set
    /// allows it; otherwise the policy chooses one
    /// (<see cref="IScheduler.SelectCpu"/>).
    /// </para>
    /// </summary>
    /// <param name="thread">The thread, <see cref="SchedulerThreadState.Created"/>, with its stack initialized.</param>
    /// <param name="affinity">The CPUs the thread may run on, or <see langword="null"/> to inherit.</param>
    /// <returns>False when the registry is full; the thread was not handed to the policy.</returns>
    internal static bool CreateThread(SchedulerThread thread, CpuSet? affinity = null)
    {
        ThrowIfDisabled();
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        Serial.WriteString("[SCHED] CreateThread: entering\n");
        using (InternalCpu.DisableInterruptsScope())
        {
            uint current = GetCurrentCpuId();
            SchedulerThread? creator = s_cpuStates[current].CurrentThread;
            CpuSet set = affinity ?? (creator is null || creator.IsIdle ? DefaultAffinity : creator.Affinity);
            if (set.IsEmpty)
            {
                set = HousekeepingCpus;
            }

            // Nothing else can see the thread yet, so no lock around these.
            thread.Affinity = set;
            thread.IdealCpu = current;
            CpuSet effective = EffectiveOf(set);
            thread.CpuId = effective.Contains(current) ? current : SelectCpuVerified(thread, effective);
            if (!effective.Contains(thread.IdealCpu))
            {
                thread.IdealCpu = thread.CpuId;
            }

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

        // The handle itself goes in UnregisterThread, under the registry lock:
        // a walk of the registry (FindByManagedThread) reads it under that
        // lock, and must never read a freed one.

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
    /// Takes back a thread that never ran: the verdict and the removal under
    /// the thread's queue lock, so the switch-in of its CPU, which turns it
    /// Running under that same lock, either came first or cannot come. True
    /// once the thread is gone from the policy and the registry; false when
    /// it began, in which case its own start frees what it holds. What
    /// <see cref="KernelThread.TryStart(Action)"/> calls when its wait ended
    /// without the thread, from whatever CPU the caller runs on.
    /// </summary>
    /// <param name="thread">The thread, expected <see cref="SchedulerThreadState.Created"/>.</param>
    internal static bool TryReapCreated(SchedulerThread thread)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        using (InternalCpu.DisableInterruptsScope())
        {
            PerCpuState state = AcquireThreadQueueLock(thread);
            if (thread.State != SchedulerThreadState.Created)
            {
                state._lock.Release();
                return false;
            }

            thread.State = SchedulerThreadState.Dead;
            s_currentScheduler.OnThreadExit(state, thread);
            state._lock.Release();

            if (GarbageCollector.IsEnabled)
            {
                GarbageCollector.ReturnAllocContext(ref thread._allocContext);
            }

            UnregisterThread(thread);
        }

        return true;
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
    /// A thread a CPU is still on does not move here (that CPU's switch-out
    /// evicts it when its set excludes the CPU, SMP phase 3b), nor an idle
    /// thread, which is its CPU's, nor any thread to a CPU its effective set
    /// excludes. <see cref="SetAffinity"/> is what the ring and the suites
    /// call; this is the primitive under it.
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
                    bool moved = MigrateLocked(thread, fromState, toState, evicting: false);

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

    /// <summary>
    /// The move itself, both queue locks held by the caller. A thread whose
    /// CPU is still on its stack (<see cref="SchedulerThread.IsOnCpu"/>)
    /// moves only when that CPU is the caller, evicting it from its own
    /// switch-out (<paramref name="evicting"/>): the one path that moves a
    /// Running thread, which turns Ready here, after the policy saw it as
    /// the thread the switch-out took off. Any other caller is refused the
    /// thread until the stub has left it, and refused a destination the
    /// thread's effective set excludes.
    /// </summary>
    private static bool MigrateLocked(SchedulerThread thread, PerCpuState fromState, PerCpuState toState, bool evicting)
    {
        if (!evicting && (thread.IsOnCpu || !EffectiveAffinity(thread).Contains(toState.CpuId)))
        {
            return false;
        }

        switch (thread.State)
        {
            case SchedulerThreadState.Running:
                if (!evicting)
                {
                    return false;
                }

                thread.CpuId = toState.CpuId;
                FollowMove(thread, toState);
                s_currentScheduler!.OnThreadMigrate(thread, fromState, toState);
                thread.State = SchedulerThreadState.Ready;
                Volatile.Write(ref toState._needReschedule, true);
                return true;

            case SchedulerThreadState.Ready:
                thread.CpuId = toState.CpuId;
                FollowMove(thread, toState);
                s_currentScheduler!.OnThreadMigrate(thread, fromState, toState);
                Volatile.Write(ref toState._needReschedule, true);
                return true;

            case SchedulerThreadState.Blocked:
            case SchedulerThreadState.Sleeping:
                thread.CpuId = toState.CpuId;
                FollowMove(thread, toState);
                return true;

            default:
                // Created: not yet handed to the policy, or queued without
                // the state saying so. Dead: gone.
                return false;
        }
    }

    /// <summary>
    /// The ideal CPU follows a move its set forced: a set that excludes the
    /// preference points it at the CPU the thread goes to, so it is always
    /// inside the set. Both queue locks held.
    /// </summary>
    private static void FollowMove(SchedulerThread thread, PerCpuState toState)
    {
        if (!thread.Affinity.Contains(thread.IdealCpu))
        {
            thread.IdealCpu = toState.CpuId;
        }
    }

    /// <summary>
    /// Moves the thread the last switch-out on <paramref name="state"/>'s CPU
    /// evicted (<see cref="PerCpuState._pendingMigration"/>): on that CPU,
    /// on the same interrupt exit, once the caller released this CPU's queue
    /// lock and before the resume guard. The policy chooses the destination
    /// under no lock; both queue locks are then taken in index order and the
    /// thread moved by its state (Running turns Ready in the destination's
    /// structure, a queued thread migrates, a parked one changes CPU), and
    /// the destination is sent the reschedule interrupt. When the set
    /// changed back in the window and allows this CPU after all, the thread
    /// goes back into this CPU's run structure instead, as a preemption
    /// would have put it.
    /// </summary>
    private static void MigratePending(PerCpuState state)
    {
        SchedulerThread? thread = state._pendingMigration;
        if (thread is null)
        {
            return;
        }

        state._pendingMigration = null;

        uint fromCpu = state.CpuId;
        CpuSet allowed = EffectiveAffinity(thread);
        uint target = allowed.Contains(fromCpu) ? fromCpu : SelectCpuVerified(thread, allowed);

        PerCpuState first = s_cpuStates![Math.Min(fromCpu, target)];
        PerCpuState second = s_cpuStates[Math.Max(fromCpu, target)];
        first._lock.Acquire();
        if (target != fromCpu)
        {
            second._lock.Acquire();
        }

        bool moved = false;
        if (thread.CpuId == fromCpu)
        {
            if (target == fromCpu)
            {
                // Back where a preemption would have put it: a Running thread
                // through the yield hook; a Ready one is queued here already
                // (the eviction put a popped one back), and a parked one is
                // in no structure until its wake queues it.
                if (thread.State == SchedulerThreadState.Running)
                {
                    thread.State = SchedulerThreadState.Ready;
                    s_currentScheduler!.OnThreadYield(state, thread);
                }

                Volatile.Write(ref state._needReschedule, true);
            }
            else
            {
                // The destination was chosen under no lock: a set that
                // changed meanwhile to exclude it is caught by that CPU's own
                // switch-out, which evicts the thread again.
                moved = MigrateLocked(thread, state, s_cpuStates[target], evicting: true);
            }
        }

        if (target != fromCpu)
        {
            second._lock.Release();
        }

        first._lock.Release();

        // The interrupt after the locks, never under them: the destination's
        // exit takes its lock first thing.
        if (moved && Volatile.Read(ref PerCpuBlock.At(target).Online) != 0)
        {
            InterruptManager.SendReschedule(PerCpuBlock.At(target).HardwareId);
        }
    }

    /// <summary>
    /// The resume guard, the last scheduler step of an interrupt exit, on
    /// the CPU taking the exit with interrupts masked and no lock held. When
    /// this exit staged a switch, waits until the thread it switches to is
    /// off the stack of the CPU that last ran it (its on-CPU marker, which
    /// that CPU's stub clears right after its own stack switch), then marks
    /// it on this CPU. The wait is bounded by that stub, which nothing holds
    /// up: a handler past its scheduler hooks takes no lock and runs masked
    /// to the switch, and a thread still marked never enters another CPU's
    /// run structure except through the eviction its own CPU runs after its
    /// pick, so two CPUs cannot wait on each other.
    /// </summary>
    /// <param name="cpuId">Current CPU ID.</param>
    internal static void FinishSwitchFromIrq(uint cpuId)
    {
        if (s_cpuStates is null || cpuId >= s_cpuCount || ContextSwitchNative.GetContextSwitchSp() == 0)
        {
            return;
        }

        SchedulerThread? next = s_cpuStates[cpuId].CurrentThread;
        if (next is null)
        {
            return;
        }

        while (Volatile.Read(ref next._onCpu) != 0)
        {
        }

        Volatile.Write(ref next._onCpu, 1);
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
    /// Asks the installed scheduler which of <paramref name="allowed"/> a
    /// thread goes to (<see cref="IScheduler.SelectCpu"/>) and checks the
    /// answer: a CPU outside the set, or past the ones managed, becomes the
    /// set's first. Under no lock, the hook's contract.
    /// </summary>
    /// <param name="thread">Thread being placed.</param>
    /// <param name="allowed">The thread's effective set, never empty.</param>
    private static uint SelectCpuVerified(SchedulerThread thread, CpuSet allowed)
    {
        uint cpu = s_currentScheduler!.SelectCpu(thread, s_cpuStates!, allowed);
        return cpu < s_cpuCount && allowed.Contains(cpu) ? cpu : allowed.First;
    }

    /// <summary>
    /// Gives the installed scheduler a chance to pull a thread onto one CPU
    /// (<see cref="IScheduler.Balance"/>) and applies its answer once it
    /// holds under both queue locks: the thread is still queued on the CPU
    /// it named, no CPU is on its stack, and its effective set allows the
    /// puller. Dormant: nothing in the kernel calls it until placement lands
    /// (SMP phase 5); the Smp suite does. Thread context, no lock on entry.
    /// </summary>
    /// <param name="cpuId">CPU asking for work.</param>
    /// <returns>True when a thread moved to <paramref name="cpuId"/>.</returns>
    internal static bool Balance(uint cpuId)
    {
        ThrowIfCpuStateNotInitialized();
        ThrowIfSchedulerNotSet();

        if (cpuId >= s_cpuCount)
        {
            return false;
        }

        PerCpuState toState = s_cpuStates[cpuId];
        SchedulerThread? victim = s_currentScheduler.Balance(toState, s_cpuStates);
        if (victim is null || victim.IsIdle)
        {
            return false;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            uint fromCpu = victim.CpuId;
            if (fromCpu == cpuId)
            {
                return false;
            }

            PerCpuState first = s_cpuStates[Math.Min(fromCpu, cpuId)];
            PerCpuState second = s_cpuStates[Math.Max(fromCpu, cpuId)];
            first._lock.Acquire();
            second._lock.Acquire();

            bool moved = victim.CpuId == fromCpu && victim.State == SchedulerThreadState.Ready
                && MigrateLocked(victim, s_cpuStates[fromCpu], toState, evicting: false);

            second._lock.Release();
            first._lock.Release();

            if (moved && cpuId != GetCurrentCpuId() && Volatile.Read(ref PerCpuBlock.At(cpuId).Online) != 0)
            {
                InterruptManager.SendReschedule(PerCpuBlock.At(cpuId).HardwareId);
            }

            return moved;
        }
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
        MigratePending(state);
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
        MigratePending(state);
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
        MigratePending(state);
    }

    /// <summary>
    /// The switch itself, the CPU's queue lock held by the caller, on that CPU, in interrupt
    /// context. The caller releases the lock and then runs <see cref="MigratePending"/>: a
    /// thread whose set excludes this CPU is evicted here (left out of the run structure,
    /// still Running when it was) and moved there, under both CPUs' locks.
    /// </summary>
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

        // The eviction (SMP phase 3b): a thread this CPU is excluded from
        // leaves now, whatever the policy picked. The cheap check first, the
        // online intersection only when the set excludes this CPU.
        bool evict = prev is not null && !prev.IsIdle && prev.State != SchedulerThreadState.Dead
            && !prev.Affinity.Contains(state.CpuId) && !EffectiveAffinity(prev).Contains(state.CpuId);
        if (evict)
        {
            state._pendingMigration = prev;
            if (next == prev)
            {
                // The policy handed prev back: run the idle thread meanwhile.
                // A Ready prev was popped from its queue by that pick and
                // goes back in, so that an evicted Ready thread is queued on
                // this CPU whichever way the pick went, which is what the
                // move (OnThreadMigrate) and a set change back both expect.
                if (next.State == SchedulerThreadState.Ready)
                {
                    s_currentScheduler.OnPickFailed(state, next);
                }

                next = state.IdleThread;
                if (next is null)
                {
                    state._pendingMigration = null;
                    return;
                }
            }
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
            // created one included, never reaches the head. An evicted prev
            // stays Running and out of the structure: MigratePending moves
            // it once this lock is released.
            if (prev is not null)
            {
                prev.StackPointer = currentRsp;
                if (prev.State == SchedulerThreadState.Running && !evict)
                {
                    prev.State = SchedulerThreadState.Ready;
                    s_currentScheduler.OnThreadYield(state, prev);
                }

                // The stub clears prev's on-CPU marker right after it has
                // switched stacks: until then a CPU that resumes prev waits.
                ContextSwitchNative.SetContextSwitchPrevOnCpu(prev.OnCpuSlot);
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

        if (prev is not null)
        {
            prev.State = SchedulerThreadState.Ready;

            // As in ScheduleFromInterruptLocked: the stub that applies this
            // switch clears prev's on-CPU marker once it has left its stack.
            ContextSwitchNative.SetContextSwitchPrevOnCpu(prev.OnCpuSlot);
        }

        next.State = SchedulerThreadState.Running;
        ContextSwitchNative.SetContextSwitchSp(next.StackPointer);
    }

    private static ulong GetTimestamp()
    {
        return (ulong)Stopwatch.GetTimestamp();
    }
}
