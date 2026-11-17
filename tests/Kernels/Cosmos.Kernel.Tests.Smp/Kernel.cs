// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Threading;
using Cosmos.Kernel.System.Timers;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using CoreCpuSet = Cosmos.Kernel.Core.Scheduler.CpuSet;
using CpuSet = Cosmos.Kernel.System.Threading.CpuSet;
using SysThread = System.Threading.Thread;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Smp;

/// <summary>
/// CPU affinity (CosmosOS/Cosmos#3341, SMP phase 3b): the CPU set and the ideal CPU a
/// thread carries, what it inherits, the ring that reads and sets them, and the mechanism
/// that moves a thread its set excludes from its CPU, running or not. The cells that move
/// a thread run on the four-CPU cell and skip on one CPU. A worker away from CPU 0 touches
/// statics only and allocates nothing, and comes back to CPU 0 before it exits: until the
/// collector stops every CPU (SMP phase 4) a collection on CPU 0 neither stops nor scans a
/// thread running elsewhere, and the exit path allocates.
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total number of tests announced to the test runner for this suite.</summary>
    private const int ExpectedTestCount = 30;

    /// <summary>CPUs the smp4 cell's machine has: what the bootloader lists and the kernel starts there.</summary>
    private const uint Smp4CpuCount = 4;

    /// <summary>Polling interval (ms) while waiting on a worker's report.</summary>
    private const int PollIntervalMs = 50;
    /// <summary>Maximum number of PollIntervalMs polls while waiting on a worker's report.</summary>
    private const int PollRetries = 100;
    /// <summary>Window (ms) a spinner's counter is sampled across to prove it runs on a CPU.</summary>
    private const int SpinWindowMs = 200;
    /// <summary>Maximum wait (ms) for a worker to exit once released on CPU 0.</summary>
    private const int JoinTimeoutMs = 30_000;
    /// <summary>Attempts at a pull through Balance, each a window apart, before the cell gives up.</summary>
    private const int BalanceAttempts = 20;
    /// <summary>
    /// Looks for a thread among what a CPU holds; a tick between two looks swaps the current
    /// thread and the queue, so a handful of looks is enough.
    /// </summary>
    private const int FindAttempts = 100;

    /// <summary>Rounds of the ping-pong between CPU 1 and CPU 2.</summary>
    private const uint PingPongRounds = 200;
    /// <summary>Start of the stack-local checksum the ping-pong carries across its moves.</summary>
    private const ulong PingPongSeed = 0x9E3779B97F4A7C15;
    /// <summary>Multiplier of that checksum, one round at a time.</summary>
    private const ulong PingPongMultiplier = 31;

    /// <summary>A CPU index the bare cell manages but does not have: what its offline-set cell pins to.</summary>
    private const uint OfflineCpu = 3;

    // ===== What the workers report, statics only =====
    private static SchedulerThread? s_workerThread;
    private static volatile bool s_stop;
    private static volatile uint s_seenCpu;
    private static uint[]? s_counters;
    private static Cosmos.Kernel.Core.Scheduler.InterruptEvent? s_gate;
    private static Cosmos.Kernel.Core.Scheduler.InterruptEvent? s_release;
    private static volatile bool s_proceed;

    private static CpuSet s_childSet;
    private static volatile uint s_childIdeal;
    private static volatile bool s_childReported;
    private static CpuSet s_inheritedSet;
    private static volatile bool s_inheritedReported;
    private static volatile bool s_kernelThreadHousekeeping;
    private static volatile uint s_kernelThreadCpu;
    private static volatile bool s_kernelThreadReported;
    private static KernelThreadInfo s_info;
    private static volatile bool s_infoReported;
    private static volatile bool s_maskedThrew;
    private static volatile uint s_selfSeenAway;
    private static volatile uint s_selfSeenHome;
    private static ulong s_pingPongChecksum;
    private static volatile uint s_pingPongWrong;
    private static volatile bool s_pingPongDone;

    protected override void BeforeRun()
    {
        Log.WriteString("[Smp] BeforeRun() reached!\n");
        Log.WriteString("[Smp] Starting tests...\n");

        bool smp = TR.ProfileContains("smp4");
        const string OneCpuCell = "one CPU on this cell";

        TR.Start("SMP Affinity Tests", expectedTests: ExpectedTestCount);

        // The set itself, on both cells
        TR.Run("CpuSet_Single_ContainsOnlyThatCpu", TestCpuSetSingle);
        TR.Run("CpuSet_WithWithout_RoundTrip", TestCpuSetWithWithout);
        TR.Run("CpuSet_Intersect_KeepsCommon", TestCpuSetIntersect);
        TR.Run("CpuSet_CountFirst_AcrossWords", TestCpuSetCountFirst);
        TR.Run("CpuSet_Equality_ByValue", TestCpuSetEquality);
        TR.Run("CpuSet_OutOfRange_Throws", TestCpuSetOutOfRange);

        // The CPUs the kernel reports, and the default set
        TR.Run("KernelCpus_ReportsCountOnlineHousekeepingCompute", () => TestKernelCpusReports(smp));
        TR.Run("KernelCpus_Current_IsBootCpuFromMain", TestKernelCpusCurrentFromMain);
        TR.Run("KernelCpus_DefaultAffinity_IsHousekeeping", TestDefaultAffinityIsHousekeeping);
        TR.Run("KernelCpus_DefaultAffinity_Set_RefusesEmptyAndUnmanaged", TestDefaultAffinitySetRefuses);

        // Inheritance at the two creation seams
        TR.Run("Thread_InheritsCreatorAffinity", TestThreadInheritsCreatorAffinity);
        TR.Run("Thread_InheritsDefaultAffinity", TestThreadInheritsDefaultAffinity);
        TR.Run("KernelThread_TryStart_IsHousekeeping", TestKernelThreadIsHousekeeping);

        // The ring over a thread
        TR.Run("KernelThreadInfo_CarriesAffinityAndIdealCpu", TestKernelThreadInfoCarriesAffinity);
        TR.Run("ThreadAffinity_UnstartedThread_ReturnsFalse", TestUnstartedThreadReturnsFalse);
        TR.Run("ThreadAffinity_ExitedThread_ReturnsFalse", TestExitedThreadReturnsFalse);
        TR.Run("ThreadAffinity_Current_Set_FromMain_Throws", TestCurrentSetFromMainThrows);
        TR.Run("ThreadAffinity_Current_Set_InterruptsMasked_Throws", TestCurrentSetInterruptsMaskedThrows);
        TR.Run("ThreadAffinity_SetIdealCpu_OutOfRange_Throws", TestSetIdealCpuOutOfRangeThrows);
        TR.Run("ThreadAffinity_Set_RefusesEmptyAndUnmanaged", TestSetRefusesEmptyAndUnmanaged);
        TR.RunIf(!smp, "Affinity_OfflineOnlySet_StaysOnOnlineCpu", TestOfflineOnlySetStaysOnOnlineCpu, "every CPU this cell manages is online");

        // The mechanism across CPUs
        TR.RunIf(smp, "Affinity_PinnedThread_RunsOnThatCpu", TestPinnedThreadRunsOnThatCpu, OneCpuCell);
        TR.RunIf(smp, "Affinity_TwoPinnedSpinners_RunAtOnce", TestTwoPinnedSpinnersRunAtOnce, OneCpuCell);
        TR.RunIf(smp, "Affinity_ChangeWhileRunning_Migrates", TestChangeWhileRunningMigrates, OneCpuCell);
        TR.RunIf(smp, "Affinity_SetCurrent_ReturnsOnNewCpu", TestSetCurrentReturnsOnNewCpu, OneCpuCell);
        TR.RunIf(smp, "Affinity_PingPong_KeepsStackIntact", TestPingPongKeepsStackIntact, OneCpuCell);
        TR.RunIf(smp, "Affinity_ParkedThread_MovesAndWakesOnNewCpu", TestParkedThreadMovesAndWakesOnNewCpu, OneCpuCell);
        TR.RunIf(smp, "Balance_IdleCpuPullsQueuedThread", TestBalanceIdleCpuPullsQueuedThread, OneCpuCell);
        TR.RunIf(smp, "ThreadAffinity_SetIdealCpu_InsideSet_Updates", TestSetIdealCpuInsideSetUpdates, OneCpuCell);
        TR.RunIf(smp, "ThreadAffinity_SetIdealCpu_ExcludedCpu_Throws", TestSetIdealCpuExcludedCpuThrows, OneCpuCell);

        TR.Finish();

        Log.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ===== The set itself =====

    private static void TestCpuSetSingle()
    {
        CpuSet set = CpuSet.Single(2);
        Assert.False(set.IsEmpty);
        Assert.Equal(1, set.Count);
        Assert.Equal(2u, set.First);
        Assert.True(set.Contains(2));
        Assert.False(set.Contains(0));
        Assert.False(set.Contains(3));
        Assert.True(CpuSet.Empty.IsEmpty);
        Assert.Equal(0, CpuSet.Empty.Count);
        Assert.Equal(CpuSet.Capacity, CpuSet.All.Count);
        Assert.Equal(256, CpuSet.Capacity);
    }

    private static void TestCpuSetWithWithout()
    {
        CpuSet set = CpuSet.Empty.With(0).With(5).With(5);
        Assert.Equal(2, set.Count, "adding a CPU twice counts it once");
        Assert.True(set.Contains(0) && set.Contains(5));

        CpuSet less = set.Without(0);
        Assert.Equal(1, less.Count);
        Assert.False(less.Contains(0));
        Assert.True(less.Contains(5));
        Assert.Equal(2, set.Count, "the set is a value: Without leaves the original alone");
        Assert.True(less.Without(7) == less, "removing a CPU not in the set changes nothing");
        Assert.True(less.Without(5).IsEmpty);
    }

    private static void TestCpuSetIntersect()
    {
        CpuSet a = CpuSet.Empty.With(0).With(1).With(70);
        CpuSet b = CpuSet.Empty.With(1).With(70).With(200);
        CpuSet common = a.Intersect(b);
        Assert.Equal(2, common.Count);
        Assert.True(common.Contains(1) && common.Contains(70));
        Assert.False(common.Contains(0) || common.Contains(200));
        Assert.True(a.Intersect(CpuSet.Empty).IsEmpty);
        Assert.True(a.Intersect(CpuSet.All) == a);
    }

    private static void TestCpuSetCountFirst()
    {
        // One CPU in each of the four words.
        CpuSet set = CpuSet.Empty.With(0).With(63).With(64).With(200);
        Assert.Equal(4, set.Count);
        Assert.Equal(0u, set.First);
        Assert.Equal(63u, set.Without(0).First);
        Assert.Equal(64u, set.Without(0).Without(63).First);
        Assert.Equal(200u, set.Without(0).Without(63).Without(64).First);
        Assert.Equal(255u, CpuSet.Single(255).First);

        bool threw = false;
        try
        {
            _ = CpuSet.Empty.First;
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        Assert.True(threw, "an empty set has no first CPU");
    }

    private static void TestCpuSetEquality()
    {
        CpuSet a = CpuSet.Empty.With(1).With(130);
        CpuSet b = CpuSet.Single(130).With(1);
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.True(a != a.With(2));
        Assert.True(CpuSet.Empty == default);
    }

    private static void TestCpuSetOutOfRange()
    {
        int threw = 0;
        try
        {
            _ = CpuSet.Single((uint)CpuSet.Capacity);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw++;
        }

        try
        {
            _ = CpuSet.Empty.With(uint.MaxValue);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw++;
        }

        try
        {
            _ = CpuSet.All.Without((uint)CpuSet.Capacity);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw++;
        }

        try
        {
            _ = CpuSet.All.Contains((uint)CpuSet.Capacity);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw++;
        }

        Assert.Equal(4, threw, "Single, With, Without and Contains refuse an index at or past Capacity");
    }

    // ===== The CPUs the kernel reports =====

    private static void TestKernelCpusReports(bool smp)
    {
        uint count = smp ? Smp4CpuCount : 1;
        CpuSet online = CpuSet.Empty;
        for (uint cpu = 0; cpu < count; cpu++)
        {
            online = online.With(cpu);
        }

        Assert.Equal((int)count, KernelCpus.Count, "KernelCpus.Count is what the bootloader listed");
        Assert.Equal(online, KernelCpus.Online, "every CPU the kernel started is online");
        Assert.Equal(CpuSet.Single(0), KernelCpus.Housekeeping);
        Assert.Equal(smp ? online.Without(0) : online, KernelCpus.Compute, "Compute is Online without CPU 0, or Online alone on one CPU");
        Assert.Equal((uint)SchedulerDiagnostics.CpuCount, (uint)KernelCpus.Count, "the diagnostics and the ring agree on the count");
    }

    private static void TestKernelCpusCurrentFromMain()
    {
        Assert.Equal(0u, KernelCpus.Current, "Main runs on the boot CPU");
        Assert.Equal(0u, (uint)SysThread.GetCurrentProcessorId());
    }

    private static void TestDefaultAffinityIsHousekeeping()
    {
        Assert.Equal(KernelCpus.Housekeeping, KernelCpus.DefaultAffinity, "nothing leaves CPU 0 until a kernel says so");
    }

    private static void TestDefaultAffinitySetRefuses()
    {
        int threw = 0;
        try
        {
            KernelCpus.DefaultAffinity = CpuSet.Empty;
        }
        catch (ArgumentException)
        {
            threw++;
        }

        try
        {
            KernelCpus.DefaultAffinity = CpuSet.Single((uint)KernelCpus.Count);
        }
        catch (ArgumentException)
        {
            threw++;
        }

        Assert.Equal(2, threw, "an empty set and a set naming no CPU under Count are refused");
        Assert.Equal(KernelCpus.Housekeeping, KernelCpus.DefaultAffinity, "a refused set leaves the default alone");
    }

    // ===== Inheritance =====

    // A set that names CPU 0 and one more: valid on both cells (it names a CPU under Count),
    // and distinct from the housekeeping set, so inheritance shows.
    private static CpuSet WidenedSet => CpuSet.Single(0).With(1);

    private static void InheritanceChild()
    {
        s_childSet = ThreadAffinity.Current;
        ThreadAffinity.TryGetIdealCpu(SysThread.CurrentThread, out uint ideal);
        s_childIdeal = ideal;
        s_childReported = true;
    }

    private static void InheritanceCreator()
    {
        ThreadAffinity.Current = WidenedSet;
        SysThread child = new(InheritanceChild);
        child.Start();
        child.Join();
    }

    // A thread takes its creator's set, and the creating CPU as its ideal CPU.
    private static void TestThreadInheritsCreatorAffinity()
    {
        s_childReported = false;
        SysThread creator = new(InheritanceCreator);
        creator.Start();
        Assert.True(creator.Join(JoinTimeoutMs), "the creator and its child exit");
        Assert.True(s_childReported, "the child ran");
        Assert.Equal(WidenedSet, s_childSet, "the child carries the set its creator had when it started it");
        Assert.Equal(0u, s_childIdeal, "the ideal CPU is the CPU the child was created on");
    }

    private static void InheritedReporter()
    {
        s_inheritedSet = ThreadAffinity.Current;
        s_inheritedReported = true;
    }

    // A thread created from Main, which runs on the boot CPU's idle thread, takes the default set.
    private static void TestThreadInheritsDefaultAffinity()
    {
        s_inheritedReported = false;
        KernelCpus.DefaultAffinity = WidenedSet;
        try
        {
            Assert.Equal(WidenedSet, KernelCpus.DefaultAffinity);
            SysThread worker = new(InheritedReporter);
            worker.Start();
            Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
        }
        finally
        {
            KernelCpus.DefaultAffinity = KernelCpus.Housekeeping;
        }

        Assert.True(s_inheritedReported, "the worker ran");
        Assert.Equal(WidenedSet, s_inheritedSet, "a thread started from Main takes the default set");
        Assert.Equal(KernelCpus.Housekeeping, KernelCpus.DefaultAffinity, "the default is back to housekeeping");
    }

    private static void KernelThreadBody()
    {
        s_kernelThreadHousekeeping = SchedulerManager.CurrentThread!.Affinity == CoreCpuSet.Single(0);
        s_kernelThreadCpu = KernelCpus.Current;
        s_kernelThreadReported = true;
    }

    // The kernel's own threads (KernelThread.TryStart: the finalizer, the driver kit's, the
    // sessions', audio) are housekeeping whatever the default set says.
    private static void TestKernelThreadIsHousekeeping()
    {
        s_kernelThreadReported = false;
        KernelCpus.DefaultAffinity = WidenedSet;
        try
        {
            Assert.True(KernelThread.TryStart(KernelThreadBody), "the kernel thread started");
        }
        finally
        {
            KernelCpus.DefaultAffinity = KernelCpus.Housekeeping;
        }

        WaitForFlag(() => s_kernelThreadReported, "the kernel thread reported");
        Assert.True(s_kernelThreadHousekeeping, "a kernel thread carries the housekeeping set, not the default");
        Assert.Equal(0u, s_kernelThreadCpu, "it ran on CPU 0");
    }

    // ===== The ring over a thread =====

    private static void InfoReporter()
    {
        while (!s_proceed)
        {
        }

        SchedulerDiagnostics.TryGetCurrentThread(KernelCpus.Current, out s_info);
        s_infoReported = true;
    }

    private static void TestKernelThreadInfoCarriesAffinity()
    {
        s_proceed = false;
        s_infoReported = false;
        SysThread worker = new(InfoReporter);
        worker.Start();
        Assert.True(ThreadAffinity.Set(worker, WidenedSet), "the running worker takes the set");
        s_proceed = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
        Assert.True(s_infoReported, "the worker snapshotted itself");
        Assert.Equal(WidenedSet, s_info.Affinity, "the snapshot carries the set");
        Assert.Equal(0u, s_info.IdealCpu, "the snapshot carries the ideal CPU");
        Assert.Equal(0u, s_info.CpuId);
        Assert.False(s_info.IsIdle);
        Assert.True(s_info.IsManaged);
    }

    private static void Noop()
    {
    }

    private static void TestUnstartedThreadReturnsFalse()
    {
        SysThread unstarted = new(Noop);
        Assert.False(ThreadAffinity.TryGet(unstarted, out CpuSet set), "a thread the scheduler is not running has no set");
        Assert.True(set.IsEmpty, "the out value is Empty then");
        Assert.False(ThreadAffinity.TryGetIdealCpu(unstarted, out uint ideal));
        Assert.Equal(0u, ideal);
        Assert.False(ThreadAffinity.Set(unstarted, CpuSet.Single(0)));
        Assert.False(ThreadAffinity.SetIdealCpu(unstarted, 0));
        Assert.False(ThreadAffinity.Pin(unstarted, 0));
    }

    private static void TestExitedThreadReturnsFalse()
    {
        SysThread worker = new(Noop);
        worker.Start();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");

        // Join returns from CoreLib's side of the exit; the scheduler's slot goes a moment later.
        bool gone = false;
        for (int i = 0; i < PollRetries && !gone; i++)
        {
            gone = !ThreadAffinity.TryGet(worker, out _);
            if (!gone)
            {
                TimerManager.Wait(PollIntervalMs);
            }
        }

        Assert.True(gone, "an exited thread no longer resolves");
        Assert.False(ThreadAffinity.Set(worker, CpuSet.Single(0)), "and cannot be given a set");
    }

    private static void TestCurrentSetFromMainThrows()
    {
        Assert.Equal(KernelCpus.Housekeeping, ThreadAffinity.Current, "the boot thread reads CPU 0's idle set");

        bool threw = false;
        try
        {
            ThreadAffinity.Current = CpuSet.Single(0);
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        Assert.True(threw, "Main runs on CPU 0's idle thread, which cannot move");
    }

    private static void MaskedSetter()
    {
        // The throw happens under the mask; the unwind through the scope
        // lifts it before the handler runs.
        try
        {
            using (InternalCpu.DisableInterruptsScope())
            {
                ThreadAffinity.Current = CpuSet.Single(0);
            }
        }
        catch (InvalidOperationException)
        {
            s_maskedThrew = true;
        }
    }

    // A self-move waits for the reschedule interrupt on the caller's CPU, which cannot land
    // while interrupts are masked: the setter refuses rather than hang.
    private static void TestCurrentSetInterruptsMaskedThrows()
    {
        s_maskedThrew = false;
        SysThread worker = new(MaskedSetter);
        worker.Start();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
        Assert.True(s_maskedThrew, "the setter throws under a masked section");
    }

    private static void TestSetIdealCpuOutOfRangeThrows()
    {
        SysThread unstarted = new(Noop);
        bool threw = false;
        try
        {
            ThreadAffinity.SetIdealCpu(unstarted, (uint)KernelCpus.Count);
        }
        catch (ArgumentOutOfRangeException)
        {
            threw = true;
        }

        Assert.True(threw, "an ideal CPU at or past Count is refused before the thread is looked at");
    }

    private static void TestSetRefusesEmptyAndUnmanaged()
    {
        SysThread unstarted = new(Noop);
        int threw = 0;
        try
        {
            ThreadAffinity.Set(unstarted, CpuSet.Empty);
        }
        catch (ArgumentException)
        {
            threw++;
        }

        try
        {
            ThreadAffinity.Set(unstarted, CpuSet.Single((uint)KernelCpus.Count));
        }
        catch (ArgumentException)
        {
            threw++;
        }

        Assert.Equal(2, threw, "an empty set and a set naming no CPU under Count are refused before the thread is looked at");
    }

    // ===== The mechanism across CPUs =====

    // Spins, reporting the CPU it runs on and counting in that CPU's slot. Statics only.
    private static void Spinner()
    {
        s_workerThread = SchedulerManager.CurrentThread;
        uint[] counters = s_counters!;
        while (!s_stop)
        {
            uint cpu = KernelCpus.Current;
            s_seenCpu = cpu;
            Interlocked.Increment(ref counters[cpu]);
        }
    }

    [MemberNotNull(nameof(s_counters), nameof(s_gate), nameof(s_release))]
    private static void ResetWorkers()
    {
        s_workerThread = null;
        s_stop = false;
        s_seenCpu = 0;
        s_counters = new uint[KernelCpus.Count];
        s_gate = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
        s_release = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
    }

    private static uint Counter(uint cpu)
    {
        return Volatile.Read(ref s_counters![cpu]);
    }

    private static SysThread StartSpinner()
    {
        SysThread worker = new(Spinner);
        worker.Start();
        return worker;
    }

    private static void WaitForFlag(Func<bool> flag, string what)
    {
        for (int i = 0; i < PollRetries && !flag(); i++)
        {
            TimerManager.Wait(PollIntervalMs);
        }

        Assert.True(flag(), what);
    }

    private static void WaitForSeenCpu(uint cpu)
    {
        for (int i = 0; i < PollRetries && s_seenCpu != cpu; i++)
        {
            TimerManager.Wait(PollIntervalMs);
        }

        Assert.Equal(cpu, s_seenCpu, "the spinner reports the CPU it was pinned to");
    }

    private static void WaitForCounter(uint cpu)
    {
        uint before = Counter(cpu);
        for (int i = 0; i < PollRetries && Counter(cpu) == before; i++)
        {
            TimerManager.Wait(PollIntervalMs);
        }

        Assert.True(Counter(cpu) != before, "a spinner counts on that CPU");
    }

    private static void WaitForIdle(uint cpu)
    {
        bool idle = false;
        for (int i = 0; i < PollRetries && !idle; i++)
        {
            idle = SchedulerDiagnostics.TryGetCurrentThread(cpu, out KernelThreadInfo info) && info.IsIdle;
            if (!idle)
            {
                TimerManager.Wait(PollIntervalMs);
            }
        }

        Assert.True(idle, "the CPU is back on its idle thread");
    }

    // The thread a CPU holds beside its idle thread, current or queued. The idle thread holds
    // tickets like any other and is queued again at every switch-out, so a pinned thread
    // shares its CPU with it tick for tick and the current thread alone is a coin toss. A tick
    // between the two looks swaps them, so the look is retried.
    private static bool TryFindWorkerOn(uint cpu, out KernelThreadInfo info)
    {
        for (int attempt = 0; attempt < FindAttempts; attempt++)
        {
            if (SchedulerDiagnostics.TryGetCurrentThread(cpu, out info) && !info.IsIdle)
            {
                return true;
            }

            int count = SchedulerDiagnostics.GetRunQueueCount(cpu);
            for (int i = 0; i < count; i++)
            {
                if (SchedulerDiagnostics.TryGetRunQueueThread(cpu, i, out info) && !info.IsIdle)
                {
                    return true;
                }
            }
        }

        info = default;
        return false;
    }

    // Pins a spinner back to CPU 0, waits until it counts there, stops it and joins: the exit
    // path allocates, so a worker exits on CPU 0 alone.
    private static void StopSpinner(SysThread worker)
    {
        Assert.True(ThreadAffinity.Pin(worker, 0), "the spinner is pinned home");
        WaitForCounter(0);
        s_stop = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the spinner exits once stopped on CPU 0");
    }

    private static void StopSpinners(SysThread first, SysThread second)
    {
        Assert.True(ThreadAffinity.Pin(first, 0) && ThreadAffinity.Pin(second, 0), "both spinners are pinned home");
        WaitForCounter(0);
        s_stop = true;
        Assert.True(first.Join(JoinTimeoutMs) && second.Join(JoinTimeoutMs), "both spinners exit once stopped on CPU 0");
    }

    // A thread pinned to another CPU runs there, keeps running there, and reports its set and
    // ideal CPU through the ring. The spinner is queued on CPU 0 when it is pinned (Main runs),
    // so this is the move of a queued thread.
    private static void TestPinnedThreadRunsOnThatCpu()
    {
        ResetWorkers();
        SysThread worker = StartSpinner();
        WaitForCounter(0);

        Assert.True(ThreadAffinity.Pin(worker, 1), "a running worker takes a pin");
        WaitForSeenCpu(1);

        uint before = Counter(1);
        TimerManager.Wait(SpinWindowMs);
        Assert.True(Counter(1) > before, "the spinner keeps counting on CPU 1");
        Assert.Equal(1u, s_seenCpu, "and never reports another CPU");
        Assert.True(ThreadAffinity.TryGet(worker, out CpuSet set));
        Assert.Equal(CpuSet.Single(1), set);
        Assert.True(ThreadAffinity.TryGetIdealCpu(worker, out uint ideal));
        Assert.Equal(1u, ideal, "the ideal CPU followed the move its set forced");

        StopSpinner(worker);
    }

    // Two spinners pinned to two application processors run at the same time: each CPU holds
    // a non-idle thread with that CPU in its set, and both keep counting across one window.
    private static void TestTwoPinnedSpinnersRunAtOnce()
    {
        ResetWorkers();
        SysThread first = StartSpinner();
        SysThread second = StartSpinner();
        WaitForCounter(0);

        Assert.True(ThreadAffinity.Pin(first, 1) && ThreadAffinity.Pin(second, 2), "both spinners take a pin");
        WaitForCounter(1);
        WaitForCounter(2);

        Assert.True(TryFindWorkerOn(1, out KernelThreadInfo onOne), "CPU 1 holds a spinner");
        Assert.True(TryFindWorkerOn(2, out KernelThreadInfo onTwo), "CPU 2 holds a spinner");
        Assert.True(onOne.State is KernelThreadState.Running or KernelThreadState.Ready, "the one on CPU 1 is runnable there");
        Assert.True(onTwo.State is KernelThreadState.Running or KernelThreadState.Ready, "the one on CPU 2 is runnable there");
        Assert.Equal(CpuSet.Single(1), onOne.Affinity);
        Assert.Equal(CpuSet.Single(2), onTwo.Affinity);
        Assert.True(onOne.Id != onTwo.Id, "two different threads");

        uint beforeOne = Counter(1);
        uint beforeTwo = Counter(2);
        TimerManager.Wait(SpinWindowMs);
        Assert.True(Counter(1) > beforeOne && Counter(2) > beforeTwo, "both keep counting across the window");

        StopSpinners(first, second);
    }

    // A thread running on CPU 1 is pinned to CPU 2 from CPU 0: CPU 1's next interrupt exit
    // evicts it, moves it under both locks and sends CPU 2 the reschedule interrupt.
    private static void TestChangeWhileRunningMigrates()
    {
        ResetWorkers();
        SysThread worker = StartSpinner();
        Assert.True(ThreadAffinity.Pin(worker, 1));
        WaitForSeenCpu(1);
        WaitForCounter(1);

        Assert.True(ThreadAffinity.Pin(worker, 2), "a thread running elsewhere takes a pin");
        WaitForSeenCpu(2);
        WaitForCounter(2);
        WaitForIdle(1);

        Assert.True(TryFindWorkerOn(2, out KernelThreadInfo info), "CPU 2 holds the worker");
        Assert.Equal(2u, info.CpuId);
        Assert.Equal(CpuSet.Single(2), info.Affinity);
        Assert.Equal(2u, info.IdealCpu, "the ideal CPU followed");
        Assert.Equal(0, SchedulerDiagnostics.GetRunQueueCount(1), "CPU 1 queues nothing");

        uint before = Counter(1);
        TimerManager.Wait(SpinWindowMs);
        Assert.Equal(before, Counter(1), "nothing counts on CPU 1 any more");

        StopSpinner(worker);
    }

    private static void SelfMover()
    {
        ThreadAffinity.Current = CpuSet.Single(1);
        s_selfSeenAway = KernelCpus.Current;
        ThreadAffinity.Current = CpuSet.Single(0);
        s_selfSeenHome = KernelCpus.Current;
    }

    // A thread that excludes its own CPU from its set returns from the setter on a CPU the
    // set names: the self-move raises the reschedule interrupt on its CPU and waits for the
    // new one.
    private static void TestSetCurrentReturnsOnNewCpu()
    {
        s_selfSeenAway = uint.MaxValue;
        s_selfSeenHome = uint.MaxValue;
        SysThread worker = new(SelfMover);
        worker.Start();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker comes home and exits");
        Assert.Equal(1u, s_selfSeenAway, "the setter returned on CPU 1");
        Assert.Equal(0u, s_selfSeenHome, "and then on CPU 0");
    }

    private static void PingPong()
    {
        ulong checksum = PingPongSeed;
        uint wrong = 0;
        for (uint round = 0; round < PingPongRounds; round++)
        {
            uint target = 1 + round % 2;
            ThreadAffinity.Current = CpuSet.Single(target);
            if (KernelCpus.Current != target)
            {
                wrong++;
            }

            checksum = checksum * PingPongMultiplier + round;
        }

        s_pingPongWrong = wrong;
        s_pingPongChecksum = checksum;
        ThreadAffinity.Current = CpuSet.Single(0);
        s_pingPongDone = true;
    }

    private static ulong ExpectedPingPongChecksum()
    {
        ulong checksum = PingPongSeed;
        for (uint round = 0; round < PingPongRounds; round++)
        {
            checksum = checksum * PingPongMultiplier + round;
        }

        return checksum;
    }

    // A thread moves itself between CPU 1 and CPU 2 two hundred times, checking a stack-local
    // value and its CPU on every round: each round resumes the thread on the CPU the other
    // one's stub is leaving, which is what the off-CPU marker orders. Without it a resume
    // could land while the other CPU still runs on the thread's stack.
    private static void TestPingPongKeepsStackIntact()
    {
        s_pingPongDone = false;
        s_pingPongWrong = uint.MaxValue;
        s_pingPongChecksum = 0;
        SysThread worker = new(PingPong);
        worker.Start();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker comes home and exits");
        Assert.True(s_pingPongDone, "the worker ran every round");
        Assert.Equal(0u, s_pingPongWrong, "every round resumed on the CPU it moved to");
        Assert.Equal(ExpectedPingPongChecksum(), s_pingPongChecksum, "the stack-local checksum survived every move");
    }

    private static void Parker()
    {
        s_workerThread = SchedulerManager.CurrentThread;
        s_gate!.Wait();
        s_seenCpu = KernelCpus.Current;
        s_release!.Wait();
    }

    // A parked thread moves at once when its set changes, and the wake that readies it goes
    // to the new CPU by interrupt, where it runs.
    private static void TestParkedThreadMovesAndWakesOnNewCpu()
    {
        ResetWorkers();
        s_seenCpu = uint.MaxValue;
        SysThread worker = new(Parker);
        worker.Start();
        WaitForFlag(() => s_workerThread is { State: SchedulerThreadState.Blocked }, "the worker parked on CPU 0");

        Assert.True(ThreadAffinity.Pin(worker, 2), "a parked worker takes a pin");
        Assert.Equal(2u, s_workerThread!.CpuId, "a parked thread is reassigned at once");
        Assert.True(s_workerThread.State == SchedulerThreadState.Blocked, "and stays parked");

        s_gate.Signal();
        WaitForFlag(() => s_seenCpu != uint.MaxValue, "the worker woke");
        Assert.Equal(2u, s_seenCpu, "it woke on the CPU it was moved to");
        WaitForFlag(() => s_workerThread is { State: SchedulerThreadState.Blocked }, "the worker parked again, on CPU 2");

        Assert.True(ThreadAffinity.Pin(worker, 0), "the worker is pinned home while parked");
        Assert.Equal(0u, s_workerThread.CpuId);
        s_release.Signal();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits on CPU 0");
    }

    // The Balance hook, dormant in the kernel until placement lands (SMP phase 5), through
    // the manager's entry: two spinners allowed on CPU 1 are queued on CPU 0 while Main runs,
    // and CPU 1, idle, pulls the tail of that queue.
    private static void TestBalanceIdleCpuPullsQueuedThread()
    {
        ResetWorkers();
        SysThread first = StartSpinner();
        SysThread second = StartSpinner();
        WaitForCounter(0);
        Assert.True(ThreadAffinity.Set(first, WidenedSet) && ThreadAffinity.Set(second, WidenedSet), "both spinners may run on CPU 1");
        Assert.Equal(0u, Counter(1), "nothing has run on CPU 1 yet");

        bool pulled = false;
        for (int i = 0; i < BalanceAttempts && !pulled; i++)
        {
            pulled = SchedulerManager.Balance(1);
            if (!pulled)
            {
                TimerManager.Wait(PollIntervalMs);
            }
        }

        Assert.True(pulled, "the idle CPU pulled a queued spinner");
        WaitForCounter(1);
        Assert.True(TryFindWorkerOn(1, out KernelThreadInfo info), "CPU 1 holds the pulled spinner");
        Assert.Equal(WidenedSet, info.Affinity);

        StopSpinners(first, second);
    }

    private static void TestSetIdealCpuInsideSetUpdates()
    {
        ResetWorkers();
        SysThread worker = StartSpinner();
        WaitForCounter(0);
        Assert.True(ThreadAffinity.Set(worker, WidenedSet));
        Assert.True(ThreadAffinity.SetIdealCpu(worker, 1), "a CPU inside the set is accepted");
        Assert.True(ThreadAffinity.TryGetIdealCpu(worker, out uint ideal));
        Assert.Equal(1u, ideal);

        TimerManager.Wait(SpinWindowMs);
        Assert.Equal(0u, s_seenCpu, "a preference moves nothing: the spinner stays on CPU 0");
        Assert.True(ThreadAffinity.SetIdealCpu(worker, 0));

        s_stop = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the spinner exits");
    }

    private static void TestSetIdealCpuExcludedCpuThrows()
    {
        ResetWorkers();
        SysThread worker = StartSpinner();
        WaitForCounter(0);

        bool threw = false;
        try
        {
            ThreadAffinity.SetIdealCpu(worker, 1);
        }
        catch (ArgumentException)
        {
            threw = true;
        }

        Assert.True(threw, "an ideal CPU outside the set is refused");
        Assert.True(ThreadAffinity.TryGetIdealCpu(worker, out uint ideal));
        Assert.Equal(0u, ideal, "and the ideal CPU is unchanged");

        s_stop = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the spinner exits");
    }

    // A set naming only CPUs that are not online: the thread runs on every online CPU
    // instead, and the ring reports the set as given. Through the seam: the ring refuses a
    // set naming no CPU under Count, which on one CPU is every set but {0}.
    private static void TestOfflineOnlySetStaysOnOnlineCpu()
    {
        ResetWorkers();
        SysThread worker = StartSpinner();
        WaitForCounter(0);
        WaitForFlag(() => s_workerThread is not null, "the spinner reported its control block");

        SchedulerManager.SetAffinity(s_workerThread!, CoreCpuSet.Single(OfflineCpu));
        uint before = Counter(0);
        TimerManager.Wait(SpinWindowMs);
        Assert.True(Counter(0) > before, "the spinner keeps running on CPU 0");
        Assert.Equal(0u, s_seenCpu);
        Assert.True(ThreadAffinity.TryGet(worker, out CpuSet set));
        Assert.Equal(CpuSet.Single(OfflineCpu), set, "the set is reported as given");

        s_stop = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the spinner exits");
    }
}
