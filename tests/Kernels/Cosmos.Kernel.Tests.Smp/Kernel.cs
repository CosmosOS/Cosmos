// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Input;
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
/// a thread run on the four-CPU cell and skip on one CPU. A worker away from CPU 0 reports
/// through statics, read on CPU 0 without a lock, and allocates there like anywhere else:
/// the collector stops every CPU (SMP phase 4), and the spinners keep an object per round
/// to prove it. A worker comes back to CPU 0 before it exits because the cells prove the
/// move each way. Two cells share the ring's structures between CPUs (CosmosOS/Cosmos#3347,
/// SMP phase 4b): the keyboard's queue filled on CPU 0 and drained on CPU 1, and the tick
/// source's registry registered into from CPUs 1 to 3 while CPU 0 ticks. The last cells are
/// placement's (CosmosOS/Cosmos#3343, SMP phase 5): a thread started from Main takes the
/// compute CPUs and the scheduler spreads such threads over them, an idle CPU pulls work
/// from a busy one, a sleeper wakes on its own CPU's tick, the thread pool sizes itself for
/// the compute CPUs, and the kernel's own threads stay on CPU 0. Since that phase a worker
/// started from Main no longer begins on CPU 0, so the cells that move a worker away from
/// CPU 0 start it under a housekeeping default (<see cref="StartOnHousekeeping"/>).
/// </summary>
public class Kernel : Sys.Kernel
{
    /// <summary>Total number of tests announced to the test runner for this suite.</summary>
    private const int ExpectedTestCount = 39;

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
    /// <summary>Times the wake cell parks its worker and wakes it again.</summary>
    private const int WakeRounds = 20;
    /// <summary>The CPU the sleep cell pins its worker to: not CPU 0, whose tick alone woke every sleeper before.</summary>
    private const uint SleepCpu = 2;
    /// <summary>Times the sleep cell's worker sleeps on its CPU.</summary>
    private const int SleepRounds = 10;
    /// <summary>How long (ms) each of those sleeps lasts: two ticks.</summary>
    private const int SleepMs = 20;
    /// <summary>Window (ms) the parallel cell's calibration counts iterations across: the work of one of its spinners.</summary>
    private const int ParallelCalibrationMs = 300;
    /// <summary>Iterations the calibration runs between two looks at the clock.</summary>
    private const int ParallelChunk = 10_000;
    /// <summary>Spinners the parallel cell runs at once: one more than the four-CPU cell has compute CPUs, so one CPU holds two until another is free to pull one.</summary>
    private const int ParallelSpinners = 4;
    /// <summary>
    /// Attempts at the parallel cell's timing. The claim is about what the machine can do, and
    /// a host that deschedules a virtual CPU for a while stretches one attempt, not all three.
    /// </summary>
    private const int ParallelAttempts = 3;
    /// <summary>Work items the pool cell queues: several rounds for a pool sized to the compute CPUs.</summary>
    private const int PoolWorkItems = 12;
    /// <summary>How long (ms) each of those work items spins, noting the CPUs it runs on.</summary>
    private const int PoolWorkMs = 150;
    /// <summary>Delay (ms) of the alarm the housekeeping cell schedules to bring the alarm thread up.</summary>
    private const int AlarmDelayMs = 20;
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

    /// <summary>Keys the typist presses on CPU 0 while the reader drains on CPU 1.</summary>
    private const int KeyboardTypedKeys = 3_000;
    /// <summary>Keys the typist lets pile up unread before it waits: under the queue's <see cref="KeyboardManager.QueueDepth"/>, past which the oldest is overwritten.</summary>
    private const int KeyboardBacklog = KeyboardManager.QueueDepth * 3 / 4;
    /// <summary>Set 1 make codes of A to J on the US layout, the keys the typist cycles through.</summary>
    private static readonly byte[] s_keyboardScanCodes = [0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24];
    /// <summary>The keys those scan codes decode to, in the same order.</summary>
    private static readonly Key[] s_keyboardKeys = [Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J];

    /// <summary>One-shot timers each registrar schedules from its CPU.</summary>
    private const int TimersPerRegistrar = 200;
    /// <summary>Longest delay (ms) a registrar gives a timer; the delays cycle from 1 up to it.</summary>
    private const int TimerMaxDelayMs = 20;
    /// <summary>Maximum wait (ms) for every timer to fire once the registrars are done.</summary>
    private const int TimerWaitMs = 5_000;

    // ===== What the workers report, statics only =====
    private static SchedulerThread? s_workerThread;
    private static volatile bool s_stop;
    private static volatile uint s_seenCpu;
    private static uint[]? s_counters;
    private static int[]?[]? s_kept;
    private static int s_keptErrors;
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

    private static volatile uint s_creatorCpuBefore;
    private static volatile uint s_creatorCpuAfter;
    private static volatile uint s_infoCpu;
    private static volatile int s_wakeCount;
    private static volatile int s_wakeCpuChanges;
    private static volatile int s_sleepsDone;
    private static volatile int s_sleepWrongCpu;
    private static long s_workIterations;
    private static ulong s_workSink;
    private static long[]? s_workFinished;
    private static int s_workNextIndex;
    private static int s_poolCpuMask;
    private static int s_poolDone;
    private static volatile uint s_alarmCpu;

    private static volatile int s_keysTyped;
    private static volatile int s_keysRead;
    private static volatile int s_keysWrong;
    private static int[]? s_timerFires;
    private static SoftwareTimer?[]? s_timerHandles;
    private static int s_timerRegistrarsNextCpu;
    private static int s_timerRegistrationsRefused;
    private static int s_timerWrongCpu;

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
        TR.Run("KernelCpus_DefaultAffinity_IsCompute", TestDefaultAffinityIsCompute);
        TR.Run("KernelCpus_DefaultAffinity_Set_RefusesEmptyAndUnmanaged", TestDefaultAffinitySetRefuses);
        TR.Run("Environment_ProcessorCount_CountsDefaultAffinity", TestProcessorCountCountsDefaultAffinity);

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
        TR.RunIf(smp, "ThreadAffinity_SetIdealCpu_InsideSet_Updates", TestSetIdealCpuInsideSetUpdates, OneCpuCell);
        TR.RunIf(smp, "ThreadAffinity_SetIdealCpu_ExcludedCpu_Throws", TestSetIdealCpuExcludedCpuThrows, OneCpuCell);

        // The ring's locks across CPUs (SMP phase 4b)
        TR.RunIf(smp, "Keyboard_EnqueueFromCpu0_ReadFromComputeCpu", TestKeyboardEnqueueFromCpu0ReadFromComputeCpu, OneCpuCell);
        TR.RunIf(smp, "Timer_RegisterFromComputeCpu_FiresOnCpu0", TestTimerRegisterFromComputeCpuFiresOnCpu0, OneCpuCell);

        // Placement and load balancing (SMP phase 5). The pool cell after the ones that
        // count on idle compute CPUs: its workers stay around once it is done.
        TR.RunIf(smp, "Placement_ThreadsFromMain_SpreadOverComputeCpus", TestThreadsFromMainSpreadOverComputeCpus, OneCpuCell);
        TR.RunIf(smp, "Placement_WokenThread_StaysOnItsCpuWhileIdle", TestWokenThreadStaysOnItsCpuWhileIdle, OneCpuCell);
        TR.RunIf(smp, "Sleep_OnComputeCpu_WakesOnItsOwnTick", TestSleepOnComputeCpuWakesOnItsOwnTick, OneCpuCell);
        TR.RunIf(smp, "Balance_IdleCpuPullsRunnable", TestBalanceIdleCpuPullsRunnable, OneCpuCell);
        TR.RunIf(smp, "Parallel_FourSpinnersFinishInUnderTwiceOneSpinnersTime", TestFourSpinnersFinishInUnderTwiceOneSpinnersTime, OneCpuCell);
        TR.RunIf(smp, "ThreadPool_WorkersSpreadAcrossCpus", TestThreadPoolWorkersSpreadAcrossCpus, OneCpuCell);
        TR.Run("DriverKit_ThreadsStayOnHousekeepingCpu", TestKernelThreadsStayOnHousekeepingCpu);

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

    private static void TestDefaultAffinityIsCompute()
    {
        Assert.Equal(KernelCpus.Compute, KernelCpus.DefaultAffinity, "a thread started from Main takes the compute CPUs until a kernel says otherwise");
        Assert.True(KernelCpus.Count == 1 || !KernelCpus.DefaultAffinity.Contains(0), "which leave CPU 0 out when there is another");
    }

    // What CoreLib sizes itself for is the default set, not the CPUs online: the thread pool's
    // workers run where that set says, and it reads the count once.
    private static void TestProcessorCountCountsDefaultAffinity()
    {
        Assert.Equal(KernelCpus.Compute.Count, Environment.ProcessorCount, "Environment.ProcessorCount is the count of the default set");
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
        Assert.Equal(KernelCpus.Compute, KernelCpus.DefaultAffinity, "a refused set leaves the default alone");
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
        s_creatorCpuBefore = KernelCpus.Current;
        child.Start();
        s_creatorCpuAfter = KernelCpus.Current;
        child.Join();
    }

    // A thread takes its creator's set, and the creating CPU as its ideal CPU. The creator's
    // set names two CPUs, so the scheduler may move it between them: it reports the CPU it
    // ran on around the start, and the child's ideal CPU is one of the two.
    private static void TestThreadInheritsCreatorAffinity()
    {
        s_childReported = false;
        SysThread creator = new(InheritanceCreator);
        creator.Start();
        Assert.True(creator.Join(JoinTimeoutMs), "the creator and its child exit");
        Assert.True(s_childReported, "the child ran");
        Assert.Equal(WidenedSet, s_childSet, "the child carries the set its creator had when it started it");
        Assert.True(s_childIdeal == s_creatorCpuBefore || s_childIdeal == s_creatorCpuAfter, "the ideal CPU is the CPU the child was created on");
        Assert.True(WidenedSet.Contains(s_childIdeal), "and is inside the child's set");
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
            KernelCpus.DefaultAffinity = KernelCpus.Compute;
        }

        Assert.True(s_inheritedReported, "the worker ran");
        Assert.Equal(WidenedSet, s_inheritedSet, "a thread started from Main takes the default set");
        Assert.Equal(KernelCpus.Compute, KernelCpus.DefaultAffinity, "the default is back to the compute CPUs");
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
            KernelCpus.DefaultAffinity = KernelCpus.Compute;
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

        // Masked: the worker's set names two CPUs, and moved between the read of its CPU
        // and the snapshot it would snapshot whatever its old CPU runs.
        using (InternalCpu.DisableInterruptsScope())
        {
            uint cpu = KernelCpus.Current;
            s_infoCpu = cpu;
            SchedulerDiagnostics.TryGetCurrentThread(cpu, out s_info);
        }

        s_infoReported = true;
    }

    private static void TestKernelThreadInfoCarriesAffinity()
    {
        s_proceed = false;
        s_infoReported = false;
        SysThread worker = StartOnHousekeeping(InfoReporter);
        Assert.True(ThreadAffinity.Set(worker, WidenedSet), "the running worker takes the set");
        s_proceed = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
        Assert.True(s_infoReported, "the worker snapshotted itself");
        Assert.Equal(WidenedSet, s_info.Affinity, "the snapshot carries the set");
        Assert.Equal(0u, s_info.IdealCpu, "the snapshot carries the ideal CPU, the CPU the worker was created on");
        Assert.Equal(s_infoCpu, s_info.CpuId, "and the CPU the worker runs on");
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

    // Spins, reporting the CPU it runs on and counting in that CPU's slot. Allocates an object
    // per round on whichever CPU it runs and checks the previous one: a collection on another
    // CPU must stop this one and scan the object it holds (SMP phase 4).
    private static void Spinner()
    {
        s_workerThread = SchedulerManager.CurrentThread;
        uint[] counters = s_counters!;
        int[]?[] kept = s_kept!;
        uint round = 0;
        while (!s_stop)
        {
            uint cpu = KernelCpus.Current;
            s_seenCpu = cpu;
            uint count = Interlocked.Increment(ref counters[cpu]);
            int[]? previous = kept[cpu];
            if (previous is not null && previous[0] != (int)cpu)
            {
                Interlocked.Increment(ref s_keptErrors);
            }

            kept[cpu] = [(int)cpu, (int)count, (int)round];
            round++;
        }
    }

    [MemberNotNull(nameof(s_counters), nameof(s_kept), nameof(s_gate), nameof(s_release))]
    private static void ResetWorkers()
    {
        s_workerThread = null;
        s_stop = false;
        s_seenCpu = 0;
        s_counters = new uint[KernelCpus.Count];
        s_kept = new int[]?[KernelCpus.Count];
        s_keptErrors = 0;
        s_gate = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
        s_release = new Cosmos.Kernel.Core.Scheduler.InterruptEvent();
    }

    private static uint Counter(uint cpu)
    {
        return Volatile.Read(ref s_counters![cpu]);
    }

    // Starts a worker whose set is CPU 0 alone, as every thread started from Main was before
    // placement: the default set is the compute CPUs now, so the worker is started under a
    // housekeeping default, put back before this returns. What the cells that move a thread
    // away from CPU 0, and back, start from.
    private static SysThread StartOnHousekeeping(ThreadStart body)
    {
        CpuSet previous = KernelCpus.DefaultAffinity;
        KernelCpus.DefaultAffinity = KernelCpus.Housekeeping;
        try
        {
            SysThread worker = new(body);
            worker.Start();
            return worker;
        }
        finally
        {
            KernelCpus.DefaultAffinity = previous;
        }
    }

    private static SysThread StartSpinner()
    {
        return StartOnHousekeeping(Spinner);
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

    // Pins a spinner back to CPU 0, waits until it counts there, stops it and joins: the cell
    // proves the move home as it proved the move away, and the objects the spinner kept on
    // every CPU it ran on are intact.
    private static void StopSpinner(SysThread worker)
    {
        Assert.True(ThreadAffinity.Pin(worker, 0), "the spinner is pinned home");
        WaitForCounter(0);
        s_stop = true;
        Assert.True(worker.Join(JoinTimeoutMs), "the spinner exits once stopped on CPU 0");
        Assert.Equal(0, s_keptErrors, "every object a spinner allocated away from CPU 0 survived the collections");
    }

    private static void StopSpinners(SysThread first, SysThread second)
    {
        Assert.True(ThreadAffinity.Pin(first, 0) && ThreadAffinity.Pin(second, 0), "both spinners are pinned home");
        WaitForCounter(0);
        s_stop = true;
        Assert.True(first.Join(JoinTimeoutMs) && second.Join(JoinTimeoutMs), "both spinners exit once stopped on CPU 0");
        Assert.Equal(0, s_keptErrors, "every object the two spinners allocated away from CPU 0 survived the collections");
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
        int[] kept = [-1];
        for (uint round = 0; round < PingPongRounds; round++)
        {
            uint target = 1 + round % 2;
            ThreadAffinity.Current = CpuSet.Single(target);
            if (KernelCpus.Current != target)
            {
                wrong++;
            }

            // An object allocated on each side and checked on the other: the collector stops
            // and scans this thread wherever it runs (SMP phase 4).
            if (kept[0] != (int)round - 1)
            {
                wrong++;
            }

            kept = [(int)round];
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
    // value, an object it allocated on the other CPU, and its CPU on every round: each round
    // resumes the thread on the CPU the other one's stub is leaving, which is what the
    // off-CPU marker orders. Without it a resume could land while the other CPU still runs
    // on the thread's stack.
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
        SysThread worker = StartOnHousekeeping(Parker);
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

    // A preference, not a move: a parked worker allowed on two CPUs is given the other one as
    // its ideal CPU and stays assigned where it is. Parked, because a runnable thread allowed
    // on two CPUs is the scheduler's to move.
    private static void TestSetIdealCpuInsideSetUpdates()
    {
        ResetWorkers();
        s_seenCpu = uint.MaxValue;
        SysThread worker = StartOnHousekeeping(Parker);
        WaitForFlag(() => s_workerThread is { State: SchedulerThreadState.Blocked }, "the worker parked on CPU 0");

        Assert.True(ThreadAffinity.Set(worker, WidenedSet));
        Assert.True(ThreadAffinity.SetIdealCpu(worker, 1), "a CPU inside the set is accepted");
        Assert.True(ThreadAffinity.TryGetIdealCpu(worker, out uint ideal));
        Assert.Equal(1u, ideal);
        Assert.Equal(0u, s_workerThread!.CpuId, "a preference moves nothing: the worker stays assigned to CPU 0");
        Assert.True(s_workerThread.State == SchedulerThreadState.Blocked, "and stays parked");
        Assert.True(ThreadAffinity.SetIdealCpu(worker, 0));

        s_gate.Signal();
        WaitForFlag(() => s_seenCpu != uint.MaxValue, "the worker woke");
        Assert.True(WidenedSet.Contains(s_seenCpu), "on a CPU of its set");
        WaitForFlag(() => s_workerThread is { State: SchedulerThreadState.Blocked }, "the worker parked again");
        s_release.Signal();
        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
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

    // ===== The ring's locks across CPUs (SMP phase 4b, CosmosOS/Cosmos#3347) =====

    // The keyboard's queue is filled from the sink caller's context on CPU 0 and drained by a
    // reader on another CPU: the typist feeds scan codes to the decoder as the keyboard's
    // handler does, pinned to CPU 0, while the reader, pinned to CPU 1, takes the keys and
    // checks them against the sequence typed. The typist paces itself on the reader's count so
    // the queue never overflows, and nothing is lost, doubled or reordered. Both workers stop
    // on the flag once the cell gives up waiting, so a failed cell leaves no thread spinning.
    private static void TestKeyboardEnqueueFromCpu0ReadFromComputeCpu()
    {
        Assert.True(KeyboardManager.IsEnabled, "the suite's kernel has the keyboard on");
        while (KeyboardManager.TryReadKey(out _))
        {
        }

        s_keysTyped = 0;
        s_keysRead = 0;
        s_keysWrong = 0;
        s_stop = false;
        SysThread reader = new(KeyboardReader);
        SysThread typist = new(KeyboardTypist);
        reader.Start();
        typist.Start();

        bool typistExited = typist.Join(JoinTimeoutMs);
        bool readerExited = reader.Join(JoinTimeoutMs);
        s_stop = true;
        Assert.True(typistExited, "the typist pressed every key");
        Assert.True(readerExited, "the reader took every key");
        Assert.Equal(KeyboardTypedKeys, s_keysRead, "every key typed on CPU 0 was read on CPU 1");
        Assert.Equal(0, s_keysWrong, "every key read is the one typed, in order");
        Assert.False(KeyboardManager.TryReadKey(out _), "no key is left in the queue");
    }

    private static void KeyboardTypist()
    {
        ThreadAffinity.Current = CpuSet.Single(0);
        for (int i = 0; i < KeyboardTypedKeys && !s_stop; i++)
        {
            while (s_keysTyped - s_keysRead >= KeyboardBacklog && !s_stop)
            {
                SysThread.Yield();
            }

            byte scanCode = s_keyboardScanCodes[i % s_keyboardScanCodes.Length];
            KeyboardManager.HandleScanCode(scanCode, released: false);
            KeyboardManager.HandleScanCode(scanCode, released: true);
            s_keysTyped = i + 1;
        }
    }

    private static void KeyboardReader()
    {
        ThreadAffinity.Current = CpuSet.Single(1);
        int wrong = 0;
        int read = 0;
        while (read < KeyboardTypedKeys && !s_stop)
        {
            if (!KeyboardManager.TryReadKey(out KeyEvent? key))
            {
                SysThread.Yield();
                continue;
            }

            if (key.Key != s_keyboardKeys[read % s_keyboardKeys.Length])
            {
                wrong++;
            }

            read++;
            s_keysRead = read;
        }

        s_keysWrong = wrong;
    }

    // The tick source's registry is walked by CPU 0's tick while threads on the other CPUs
    // register one-shot timers into it: each registrar, pinned to CPU 1, 2 or 3, schedules a
    // batch of short timers whose callbacks count in its slot, and every one of them fires
    // exactly once, on CPU 0; a fired timer is no longer cancellable.
    private static void TestTimerRegisterFromComputeCpuFiresOnCpu0()
    {
        int registrars = (int)Smp4CpuCount - 1;
        s_timerFires = new int[Smp4CpuCount];
        s_timerHandles = new SoftwareTimer?[registrars * TimersPerRegistrar];
        s_timerRegistrarsNextCpu = 0;
        s_timerRegistrationsRefused = 0;
        s_timerWrongCpu = 0;

        SysThread[] workers = new SysThread[registrars];
        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = new SysThread(TimerRegistrar);
            workers[i].Start();
        }

        foreach (SysThread worker in workers)
        {
            Assert.True(worker.Join(JoinTimeoutMs), "a registrar scheduled its timers and exited");
        }

        Assert.Equal(0, s_timerRegistrationsRefused, "every registration returned a handle");

        int expected = registrars * TimersPerRegistrar;
        int fired = 0;
        for (int waited = 0; waited < TimerWaitMs && fired < expected; waited += PollIntervalMs)
        {
            TimerManager.Wait(PollIntervalMs);
            fired = 0;
            for (int cpu = 1; cpu <= registrars; cpu++)
            {
                fired += Volatile.Read(ref s_timerFires[cpu]);
            }
        }

        for (int cpu = 1; cpu <= registrars; cpu++)
        {
            Assert.Equal(TimersPerRegistrar, Volatile.Read(ref s_timerFires[cpu]), "every timer a registrar scheduled fired exactly once");
        }

        Assert.Equal(0, Volatile.Read(ref s_timerWrongCpu), "every callback ran on CPU 0, the tick's");

        int stillActive = 0;
        int cancellable = 0;
        foreach (SoftwareTimer? handle in s_timerHandles)
        {
            if (handle is { IsActive: true })
            {
                stillActive++;
            }

            if (TimerManager.Cancel(handle))
            {
                cancellable++;
            }
        }

        Assert.Equal(0, stillActive, "no timer is still pending");
        Assert.Equal(0, cancellable, "a fired timer is no longer cancellable");
    }

    private static void TimerRegistrar()
    {
        int cpu = Interlocked.Increment(ref s_timerRegistrarsNextCpu);
        ThreadAffinity.Current = CpuSet.Single((uint)cpu);
        int[] fires = s_timerFires!;
        SoftwareTimer?[] handles = s_timerHandles!;
        for (int i = 0; i < TimersPerRegistrar; i++)
        {
            TimeSpan delay = TimeSpan.FromMilliseconds(1 + i % TimerMaxDelayMs);
            SoftwareTimer? handle = TimerManager.Schedule(() =>
            {
                if (KernelCpus.Current != 0)
                {
                    Interlocked.Increment(ref s_timerWrongCpu);
                }

                Interlocked.Increment(ref fires[cpu]);
            }, delay);
            if (handle is null)
            {
                Interlocked.Increment(ref s_timerRegistrationsRefused);
                continue;
            }

            handles[(cpu - 1) * TimersPerRegistrar + i] = handle;
        }
    }

    // ===== Placement and load balancing (SMP phase 5, CosmosOS/Cosmos#3343) =====

    // Three spinners started from Main with nothing else said: each takes the default set,
    // the compute CPUs, and the scheduler has one on each of them, since a CPU that runs a
    // spinner is clearly busier than one that runs none. Nothing of theirs runs on CPU 0.
    private static void TestThreadsFromMainSpreadOverComputeCpus()
    {
        ResetWorkers();
        SysThread[] workers = new SysThread[Smp4CpuCount - 1];
        for (int i = 0; i < workers.Length; i++)
        {
            workers[i] = new SysThread(Spinner);
            workers[i].Start();
        }

        for (uint cpu = 1; cpu < Smp4CpuCount; cpu++)
        {
            WaitForCounter(cpu);
        }

        foreach (SysThread worker in workers)
        {
            Assert.True(ThreadAffinity.TryGet(worker, out CpuSet set), "a spinner resolves");
            Assert.Equal(KernelCpus.Compute, set, "a thread started from Main carries the compute set");
        }

        uint[] before = new uint[Smp4CpuCount];
        for (uint cpu = 1; cpu < Smp4CpuCount; cpu++)
        {
            before[cpu] = Counter(cpu);
        }

        TimerManager.Wait(SpinWindowMs);
        for (uint cpu = 1; cpu < Smp4CpuCount; cpu++)
        {
            Assert.True(Counter(cpu) > before[cpu], "every compute CPU keeps running a spinner");
        }

        Assert.Equal(0u, Counter(0), "no spinner ran on the housekeeping CPU");

        s_stop = true;
        foreach (SysThread worker in workers)
        {
            Assert.True(worker.Join(JoinTimeoutMs), "a spinner exits once stopped");
        }

        Assert.Equal(0, s_keptErrors, "every object the spinners allocated survived the collections");
    }

    private static void WakeLoop()
    {
        s_workerThread = SchedulerManager.CurrentThread;
        uint last = uint.MaxValue;
        int changes = 0;
        for (int round = 0; round < WakeRounds; round++)
        {
            s_gate!.Wait();
            uint cpu = KernelCpus.Current;
            if (last != uint.MaxValue && cpu != last)
            {
                changes++;
            }

            last = cpu;
            s_seenCpu = cpu;
            s_wakeCpuChanges = changes;
            s_wakeCount = round + 1;
        }
    }

    // A thread allowed on every compute CPU parks and is woken from CPU 0 twenty times while
    // those CPUs have nothing else to run. Every wake asks the policy where the thread goes,
    // and it stays on the CPU it last ran on, where its data is: no other CPU is lighter.
    private static void TestWokenThreadStaysOnItsCpuWhileIdle()
    {
        ResetWorkers();
        s_seenCpu = uint.MaxValue;
        s_wakeCount = 0;
        s_wakeCpuChanges = 0;
        SysThread worker = new(WakeLoop);
        worker.Start();

        for (int round = 0; round < WakeRounds; round++)
        {
            int parkedAfter = round;
            WaitForFlag(
                () => s_wakeCount == parkedAfter && s_workerThread is { State: SchedulerThreadState.Blocked },
                "the worker parked");
            s_gate.Signal();
        }

        Assert.True(worker.Join(JoinTimeoutMs), "the worker exits");
        Assert.Equal(WakeRounds, s_wakeCount, "the worker woke every time");
        Assert.True(KernelCpus.Compute.Contains(s_seenCpu), "on a compute CPU");
        Assert.Equal(0, s_wakeCpuChanges, "and on the same one: every wake left it on the CPU it last ran on");
    }

    private static void Sleeper()
    {
        ThreadAffinity.Current = CpuSet.Single(SleepCpu);
        int wrong = 0;
        for (int i = 0; i < SleepRounds; i++)
        {
            SysThread.Sleep(SleepMs);
            if (KernelCpus.Current != SleepCpu)
            {
                wrong++;
            }

            s_sleepsDone = i + 1;
        }

        s_sleepWrongCpu = wrong;
    }

    // A thread pinned to CPU 2 sleeps ten times. Each timeout is found by CPU 2's own tick,
    // which walks the registry for the sleepers assigned to it; CPU 0's tick wakes only its
    // own, so a sleeper nobody's tick looks for would never come back.
    private static void TestSleepOnComputeCpuWakesOnItsOwnTick()
    {
        s_sleepsDone = 0;
        s_sleepWrongCpu = -1;
        long start = Stopwatch.GetTimestamp();
        SysThread worker = new(Sleeper);
        worker.Start();
        Assert.True(worker.Join(JoinTimeoutMs), "every sleep ended");
        long elapsedMs = (Stopwatch.GetTimestamp() - start) * 1000 / Stopwatch.Frequency;

        Assert.Equal(SleepRounds, s_sleepsDone, "the sleeper slept every round");
        Assert.Equal(0, s_sleepWrongCpu, "and woke on the CPU it is pinned to every time");
        Assert.True(elapsedMs >= SleepRounds * SleepMs, "not before its timeouts");
    }

    // Two spinners on CPU 0 become allowed on CPU 1 as well, and nothing is asked of the
    // scheduler: CPU 1, about to run its idle thread, pulls one of the two queued on CPU 0
    // by itself, and from then on each CPU runs one.
    private static void TestBalanceIdleCpuPullsRunnable()
    {
        ResetWorkers();
        SysThread first = StartSpinner();
        SysThread second = StartSpinner();
        WaitForCounter(0);
        Assert.Equal(0u, Counter(1), "nothing has run on CPU 1 yet");

        Assert.True(ThreadAffinity.Set(first, WidenedSet) && ThreadAffinity.Set(second, WidenedSet), "both spinners may run on CPU 1");
        WaitForCounter(1);
        Assert.True(TryFindWorkerOn(1, out KernelThreadInfo info), "CPU 1 holds the spinner it pulled");
        Assert.Equal(WidenedSet, info.Affinity);

        uint beforeZero = Counter(0);
        uint beforeOne = Counter(1);
        TimerManager.Wait(SpinWindowMs);
        Assert.True(Counter(0) > beforeZero && Counter(1) > beforeOne, "one spinner stays on CPU 0 and the other runs on CPU 1");

        StopSpinners(first, second);
    }

    private static void Calibrator()
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * ParallelCalibrationMs / 1000;
        long iterations = 0;
        ulong x = PingPongSeed;
        while (Stopwatch.GetTimestamp() < deadline)
        {
            for (int i = 0; i < ParallelChunk; i++)
            {
                x = x * PingPongMultiplier + (ulong)i;
            }

            iterations += ParallelChunk;
        }

        s_workSink = x;
        Volatile.Write(ref s_workIterations, iterations);
    }

    // The same arithmetic the calibration counted, a fixed number of times. No allocation: a
    // collection stops every CPU, and would charge every spinner the same pause.
    private static void FixedWork()
    {
        long iterations = Volatile.Read(ref s_workIterations);
        ulong x = PingPongSeed;
        for (long i = 0; i < iterations; i++)
        {
            x = x * PingPongMultiplier + (ulong)i;
        }

        s_workSink = x;
        int index = Interlocked.Increment(ref s_workNextIndex) - 1;
        Volatile.Write(ref s_workFinished![index], Stopwatch.GetTimestamp());
    }

    // Starts that many spinners of the fixed work from Main and returns how long the last one
    // took to finish, in Stopwatch ticks.
    private static long TimeFixedWork(int spinners)
    {
        s_workFinished = new long[spinners];
        s_workNextIndex = 0;
        SysThread[] workers = new SysThread[spinners];
        for (int i = 0; i < spinners; i++)
        {
            workers[i] = new SysThread(FixedWork);
        }

        long start = Stopwatch.GetTimestamp();
        foreach (SysThread worker in workers)
        {
            worker.Start();
        }

        foreach (SysThread worker in workers)
        {
            Assert.True(worker.Join(JoinTimeoutMs), "a spinner finished its work");
        }

        long last = start;
        for (int i = 0; i < spinners; i++)
        {
            last = Math.Max(last, Volatile.Read(ref s_workFinished[i]));
        }

        return last - start;
    }

    // One spinner does a fixed amount of work; then four do that amount each, at once. On one
    // CPU the four would take four times as long. Spread over the three compute CPUs, with the
    // CPU that holds two relieved by the first one that goes idle, they take well under twice.
    private static void TestFourSpinnersFinishInUnderTwiceOneSpinnersTime()
    {
        s_workIterations = 0;
        SysThread calibrator = new(Calibrator);
        calibrator.Start();
        Assert.True(calibrator.Join(JoinTimeoutMs), "the calibration ended");
        Assert.True(Volatile.Read(ref s_workIterations) > 0, "and counted some work");

        long one = 0;
        long four = 0;
        bool under = false;
        for (int attempt = 0; attempt < ParallelAttempts && !under; attempt++)
        {
            one = TimeFixedWork(1);
            four = TimeFixedWork(ParallelSpinners);
            under = four < 2 * one;
        }

        long frequency = Stopwatch.Frequency;
        Log.WriteString($"[Smp] One spinner: {one * 1000 / frequency} ms, four at once: {four * 1000 / frequency} ms\n");
        Assert.True(under, "four spinners finish in under twice the time one takes: they ran on several CPUs at once");
    }

    private static void PoolWork(object? state)
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency * PoolWorkMs / 1000;
        while (Stopwatch.GetTimestamp() < deadline)
        {
            Interlocked.Or(ref s_poolCpuMask, 1 << (int)KernelCpus.Current);
        }

        Interlocked.Increment(ref s_poolDone);
    }

    // CoreLib's thread pool reads Environment.ProcessorCount, the count of the default set, and
    // keeps that many workers; started from Main they take the compute CPUs, and a dozen work
    // items that each spin for a while end up running on more than one of them, none on CPU 0.
    private static void TestThreadPoolWorkersSpreadAcrossCpus()
    {
        ThreadPool.GetMinThreads(out int minWorkers, out _);
        Assert.Equal(Environment.ProcessorCount, minWorkers, "the pool keeps a worker per CPU of the default set");

        s_poolCpuMask = 0;
        s_poolDone = 0;
        for (int i = 0; i < PoolWorkItems; i++)
        {
            Assert.True(ThreadPool.QueueUserWorkItem(PoolWork), "a work item is queued");
        }

        for (int waited = 0; waited < JoinTimeoutMs && Volatile.Read(ref s_poolDone) < PoolWorkItems; waited += PollIntervalMs)
        {
            TimerManager.Wait(PollIntervalMs);
        }

        Assert.Equal(PoolWorkItems, Volatile.Read(ref s_poolDone), "every work item ran");
        int mask = Volatile.Read(ref s_poolCpuMask);
        Assert.True((mask & 1) == 0, "no work item ran on the housekeeping CPU");
        Assert.True(BitOperations.PopCount((uint)mask) >= 2, "the pool's workers ran on more than one CPU");
    }

    // What placement does not move: the kernel's own threads (the driver kit's worker and its
    // drivers' threads, the finalizer, the alarm thread) carry the housekeeping set and sit on
    // CPU 0, where every device interrupt and the timers' callbacks are, after everything the
    // cells above spread over the other CPUs. An alarm is scheduled first, from Main, so the
    // alarm thread exists: its callback runs on CPU 0 too.
    private static void TestKernelThreadsStayOnHousekeepingCpu()
    {
        s_alarmCpu = uint.MaxValue;
        ulong alarm = AlarmManager.Schedule(static () => s_alarmCpu = KernelCpus.Current, TimeSpan.FromMilliseconds(AlarmDelayMs));
        Assert.True(alarm != 0, "the alarm is scheduled");
        WaitForFlag(() => s_alarmCpu != uint.MaxValue, "the alarm fired");
        Assert.Equal(0u, s_alarmCpu, "an alarm's callback runs on the housekeeping CPU");

        int kernelThreads = 0;
        int away = 0;
        for (int slot = 0; slot < SchedulerDiagnostics.ThreadSlotCount; slot++)
        {
            if (!SchedulerDiagnostics.TryGetThreadInSlot(slot, out KernelThreadInfo info) || info.IsIdle || info.IsManaged)
            {
                continue;
            }

            kernelThreads++;
            if (info.Affinity != KernelCpus.Housekeeping || info.CpuId != 0)
            {
                away++;
            }
        }

        Assert.True(kernelThreads >= 2, "the driver kit's worker and the alarm thread, at least");
        Assert.Equal(0, away, "every thread of the kernel's own carries the housekeeping set and is assigned to CPU 0");
    }
}
