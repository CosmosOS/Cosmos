// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Memory.Heap;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.System.Timers;
using Cosmos.TestRunner.Framework;
using CoreGC = Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.GarbageCollector;

/// <summary>
/// Finalizer thread tests: <see cref="CoreGC.ThreadedFinalizer"/> started, stopped and restarted,
/// and the finalizer behaviour <see cref="FinalizerTests"/> checks on the calling thread, checked
/// again with the finalizers run by the dedicated thread.
/// </summary>
/// <remarks>
/// Same probe discipline as <see cref="FinalizerTests"/>: each probe reports through an array the
/// test owns, and is created in a <see cref="MethodImplOptions.NoInlining"/> helper. Every probe
/// also records the scheduler thread that ran its finalizer.
/// </remarks>
internal static class FinalizerThreadTests
{
    /// <summary>
    /// Number of tests <see cref="RunAll"/> registers; the suite adds it to its expected count.
    /// </summary>
    public const int TestCount = 13;

    private static uint s_finalizerThreadId;
    private static int s_workerAllocated;
    private static int s_workerFinalized;
    private static int s_workerDone;

    public static void RunAll()
    {
        TR.Run("GC_FinalizerThread_Start", TestStart);
        TR.Run("GC_FinalizerThread_RunsOnOwnThread", TestRunsOnOwnThread);
        TR.Run("GC_FinalizerThread_StartTwiceKeepsThread", TestStartTwiceKeepsThread);
        TR.Run("GC_FinalizerThread_WaitWaitsForSlowFinalizers", TestWaitWaitsForSlowFinalizers);
        TR.Run("GC_FinalizerThread_MaskedCollect", TestMaskedCollect);
        TR.Run("GC_FinalizerThread_AllocInFinalizer", TestAllocInFinalizer);
        TR.Run("GC_FinalizerThread_CollectInFinalizer", TestCollectInFinalizer);
        TR.Run("GC_FinalizerThread_ThrowingFinalizer", TestThrowingFinalizer);
        TR.Run("GC_FinalizerThread_ConcurrentAllocators", TestConcurrentAllocators);
        TR.Run("GC_FinalizerThread_Stop", TestStop);
        TR.Run("GC_FinalizerThread_Restart", TestRestart);
        TR.Run("GC_FinalizerThread_StopFromAnotherThread", TestStopFromAnotherThread);

        // Last: a regression hangs the CPU (a masked wait on the monitor), and no later test would run.
        TR.Run("GC_FinalizerThread_AllocWhileMasked", TestAllocWhileMasked);

        CoreGC.ThreadedFinalizer = false;
    }

    /// <summary>
    /// Counts its finalization in <c>counter[0]</c> and stores the id of the thread that ran it
    /// in <c>counter[1]</c>.
    /// </summary>
    private class ThreadProbe
    {
        protected readonly uint[] Counter;

        public ThreadProbe(uint[] counter)
        {
            Counter = counter;
        }

        ~ThreadProbe()
        {
            Counter[0]++;
            Counter[1] = CurrentThreadId();
        }
    }

    /// <summary>
    /// Sleeps before it counts: a wait that does not wait for the pass returns while it sleeps.
    /// </summary>
    private sealed class SlowProbe : ThreadProbe
    {
        public SlowProbe(uint[] counter) : base(counter)
        {
        }

        ~SlowProbe()
        {
            TimerManager.Wait(5);
        }
    }

    private sealed class AllocatingProbe
    {
        private readonly uint[] _result;

        public AllocatingProbe(uint[] result)
        {
            _result = result;
        }

        ~AllocatingProbe()
        {
            int sum = 0;
            for (int i = 0; i < 64; i++)
            {
                byte[] buffer = new byte[512];
                buffer[i] = (byte)(i + 1);
                sum += buffer[i];
            }

            _result[0] = sum == 2080 ? 1u : 2u;
            _result[1] = CurrentThreadId();
        }
    }

    private sealed class CollectingProbe
    {
        private readonly uint[] _counter;

        public CollectingProbe(uint[] counter)
        {
            _counter = counter;
        }

        ~CollectingProbe()
        {
            _counter[0]++;
            GC.Collect();
            _counter[1]++;
        }
    }

    private sealed class ThrowingProbe
    {
        private readonly uint[] _counter;

        public ThrowingProbe(uint[] counter)
        {
            _counter = counter;
        }

        ~ThrowingProbe()
        {
            _counter[0]++;
            throw new InvalidOperationException("finalizer failure");
        }
    }

    private sealed class WorkerProbe
    {
        ~WorkerProbe()
        {
            Interlocked.Increment(ref s_workerFinalized);
        }
    }

    private static uint CurrentThreadId()
    {
        // White-box read of the experimental scheduler seam: the finalizer thread is a kernel
        // thread, with no managed Thread to ask.
#pragma warning disable COSMOS0001
        return SchedulerManager.CurrentCpuState?.CurrentThread?.Id ?? 0;
#pragma warning restore COSMOS0001
    }

    private static void Collect(int passes)
    {
        for (int i = 0; i < passes; i++)
        {
            CoreGC.Collect();
        }
    }

    /// <summary>
    /// Collects, then waits for the finalizer thread to run what the collection queued.
    /// </summary>
    private static void CollectAndWait(int passes)
    {
        Collect(passes);
        GC.WaitForPendingFinalizers();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateThreadProbes(uint[] counter, int count)
    {
        for (int i = 0; i < count; i++)
        {
            object probe = new ThreadProbe(counter);
            if (probe is null)
            {
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSlowProbes(uint[] counter, int count)
    {
        for (int i = 0; i < count; i++)
        {
            object probe = new SlowProbe(counter);
            if (probe is null)
            {
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAllocatingProbe(uint[] result)
    {
        object probe = new AllocatingProbe(result);
        if (probe is null)
        {
            result[0] = 2;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateCollectingProbe(uint[] counter)
    {
        object probe = new CollectingProbe(counter);
        if (probe is null)
        {
            counter[0] = 100;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateThrowingProbe(uint[] counter)
    {
        object probe = new ThrowingProbe(counter);
        if (probe is null)
        {
            counter[0] = 100;
        }
    }

    private static void TestStart()
    {
        Assert.False(CoreGC.ThreadedFinalizer, "FinalizerThread: off until a kernel turns it on");

        CoreGC.ThreadedFinalizer = true;

        Assert.True(CoreGC.ThreadedFinalizer, "FinalizerThread: setting ThreadedFinalizer starts the thread");
    }

    private static void TestRunsOnOwnThread()
    {
        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);

        CollectAndWait(1);

        uint caller = CurrentThreadId();
        Assert.Equal(1u, counter[0], "FinalizerThread: GC.WaitForPendingFinalizers returns once the finalizer ran");
        Assert.True(counter[1] != 0 && counter[1] != caller,
            "FinalizerThread: the finalizer runs on the finalizer thread, not on the caller (" + counter[1] + " vs " + caller + ")");

        s_finalizerThreadId = counter[1];
    }

    private static void TestStartTwiceKeepsThread()
    {
        CoreGC.ThreadedFinalizer = true;
        Assert.True(CoreGC.ThreadedFinalizer, "FinalizerThread: still on after being turned on twice");

        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);
        CollectAndWait(1);

        Assert.Equal(1u, counter[0], "FinalizerThread: the finalizer ran");
        Assert.Equal(s_finalizerThreadId, counter[1], "FinalizerThread: turning it on again does not start a second thread");
    }

    private static void TestWaitWaitsForSlowFinalizers()
    {
        const int count = 10;
        uint[] counter = new uint[2];
        CreateSlowProbes(counter, count);

        CollectAndWait(1);

        // The conservative scan may falsely retain one or two of them.
        Assert.True(counter[0] >= count - 2,
            "FinalizerThread: GC.WaitForPendingFinalizers waits for the whole pass, got " + counter[0] + " of " + count);
    }

    private static void TestMaskedCollect()
    {
        // A collection run with interrupts masked signals the thread from the masked section's end.
        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);

        Heap.Collect();
        GC.WaitForPendingFinalizers();

        Assert.Equal(1u, counter[0], "FinalizerThread: what a masked collection queued is finalized");
        Assert.Equal(s_finalizerThreadId, counter[1], "FinalizerThread: by the finalizer thread");
    }

    private static void TestAllocInFinalizer()
    {
        // An allocation made by the finalizer thread must not wait for the finalizer thread.
        uint[] result = new uint[2];
        CreateAllocatingProbe(result);

        CollectAndWait(1);

        Assert.Equal(1u, result[0], "FinalizerThread: a finalizer may allocate on the finalizer thread");
        Assert.Equal(s_finalizerThreadId, result[1], "FinalizerThread: the allocating finalizer ran on the finalizer thread");
    }

    private static void TestCollectInFinalizer()
    {
        uint[] counter = new uint[2];
        CreateCollectingProbe(counter);

        CollectAndWait(2);

        Assert.Equal(1u, counter[0], "FinalizerThread: a finalizer calling GC.Collect runs once");
        Assert.Equal(1u, counter[1], "FinalizerThread: GC.Collect returns inside a finalizer on the finalizer thread");

        uint[] after = new uint[2];
        CreateThreadProbes(after, 1);
        CollectAndWait(1);
        Assert.Equal(1u, after[0], "FinalizerThread: the thread keeps finalizing after a finalizer collected");
    }

    private static void TestThrowingFinalizer()
    {
        uint[] counter = new uint[2];
        uint[] throwing = new uint[1];
        CreateThreadProbes(counter, 2);
        CreateThrowingProbe(throwing);
        CreateThreadProbes(counter, 2);

        CollectAndWait(2);

        Assert.Equal(1u, throwing[0], "FinalizerThread: the throwing finalizer ran once");
        Assert.Equal(4u, counter[0], "FinalizerThread: a throwing finalizer does not stop the others");

        CreateThreadProbes(counter, 1);
        CollectAndWait(1);
        Assert.Equal(5u, counter[0], "FinalizerThread: the thread survives a finalizer that threw");
        Assert.Equal(s_finalizerThreadId, counter[1], "FinalizerThread: and it is still the same thread");
    }

    private static void AllocationWorker()
    {
        for (int i = 0; i < 2000; i++)
        {
            object probe = new WorkerProbe();
            if (probe is not null)
            {
                Interlocked.Increment(ref s_workerAllocated);
            }
        }

        Interlocked.Increment(ref s_workerDone);
    }

    private static void TestConcurrentAllocators()
    {
        // Allocating threads wait for the finalizer thread whenever a collection left work queued.
        s_workerAllocated = 0;
        s_workerFinalized = 0;
        s_workerDone = 0;

        Thread worker1 = new Thread(AllocationWorker);
        Thread worker2 = new Thread(AllocationWorker);
        worker1.Start();
        worker2.Start();

        for (int i = 0; i < 400 && s_workerDone < 2; i++)
        {
            CoreGC.Collect();
            TimerManager.Wait(5);
        }

        Assert.Equal(2, s_workerDone, "FinalizerThread: both allocating workers finish");

        CollectAndWait(3);

        int allocated = s_workerAllocated;
        int finalized = s_workerFinalized;
        Assert.Equal(4000, allocated, "FinalizerThread: every worker allocation succeeded");
        Assert.True(finalized <= allocated,
            "FinalizerThread: no object finalized twice (" + finalized + " of " + allocated + ")");
        Assert.True(finalized >= allocated - 8,
            "FinalizerThread: no object left unfinalized (" + finalized + " of " + allocated + ")");
    }

    private static void TestStop()
    {
        CoreGC.ThreadedFinalizer = false;
        Assert.False(CoreGC.ThreadedFinalizer, "FinalizerThread: setting ThreadedFinalizer to false stops the thread");

        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);
        Collect(1);

        Assert.Equal(1u, counter[0], "FinalizerThread: once stopped, a collection runs the finalizers itself");
        Assert.Equal(CurrentThreadId(), counter[1], "FinalizerThread: on the collecting thread");
    }

    private static void TestRestart()
    {
        CoreGC.ThreadedFinalizer = true;
        Assert.True(CoreGC.ThreadedFinalizer, "FinalizerThread: the thread starts again after a stop");

        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);
        CollectAndWait(1);

        uint caller = CurrentThreadId();
        Assert.Equal(1u, counter[0], "FinalizerThread: the restarted thread finalizes");
        Assert.True(counter[1] != 0 && counter[1] != caller, "FinalizerThread: on a thread of its own");
        Assert.True(counter[1] != s_finalizerThreadId, "FinalizerThread: a new thread, the stopped one exited");

        s_finalizerThreadId = counter[1];
    }

    private static void Stopper()
    {
        CoreGC.ThreadedFinalizer = false;
        Interlocked.Increment(ref s_workerDone);
    }

    private static void TestStopFromAnotherThread()
    {
        s_workerDone = 0;
        Thread stopper = new Thread(Stopper);
        stopper.Start();

        for (int i = 0; i < 200 && s_workerDone == 0; i++)
        {
            TimerManager.Wait(5);
        }

        Assert.Equal(1, s_workerDone, "FinalizerThread: a thread other than the starter stops it");
        Assert.False(CoreGC.ThreadedFinalizer, "FinalizerThread: and it reads as stopped everywhere");

        CoreGC.ThreadedFinalizer = true;
        Assert.True(CoreGC.ThreadedFinalizer, "FinalizerThread: and it starts again afterwards");
    }

    private static void TestAllocWhileMasked()
    {
        // An allocation made with interrupts masked and finalizers queued (what an IRQ handler or
        // a spin-locked section does right after a collection) must neither block nor run them.
        uint[] counter = new uint[2];
        CreateThreadProbes(counter, 1);

        uint ranInside;
        bool allocated;
        using (InternalCpu.DisableInterruptsScope())
        {
            // Masked from the collection on: the finalizer thread cannot drain the queue before
            // the allocation below sees it.
            CoreGC.Collect();
            object fresh = new object();
            allocated = fresh is not null;
            ranInside = counter[0];
        }

        GC.WaitForPendingFinalizers();

        Assert.True(allocated, "FinalizerThread: allocating with interrupts masked and finalizers queued returns");
        Assert.Equal(0u, ranInside, "FinalizerThread: no finalizer runs inside the masked section");
        Assert.Equal(1u, counter[0], "FinalizerThread: the finalizer thread runs it once interrupts are back");
    }
}
