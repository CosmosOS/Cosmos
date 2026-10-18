using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Memory.Heap;
using Cosmos.Kernel.System.Timers;
using Cosmos.TestRunner.Framework;
using CoreGC = Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.GarbageCollector;

/// <summary>
/// Finalizer (<c>~T()</c>) tests: when a finalizer runs, how often, what it may touch and do,
/// and how it interacts with <see cref="GC.SuppressFinalize"/>,
/// <see cref="GC.ReRegisterForFinalize"/>, resurrection and weak references.
/// </summary>
/// <remarks>
/// Every probe reports through an <c>int[]</c> the test owns rather than a shared static, so a
/// probe an earlier test left behind (a false root of the conservative scan) cannot be counted by
/// a later one. Probes are created in <see cref="MethodImplOptions.NoInlining"/> helpers so no
/// reference to them stays in the frame of the test.
/// </remarks>
internal static class FinalizerTests
{
    /// <summary>
    /// Number of tests <see cref="RunAll"/> registers; the suite adds it to its expected count.
    /// </summary>
    public const int TestCount = 26;

    private const int Pattern = 0x5F1A0000;

    private static object? s_resurrected;
    private static ConditionalWeakTable<object, int[]>? s_table;
    private static int s_genericFinalized;
    private static int s_workerAllocated;
    private static int s_workerFinalized;
    private static int s_workerDone;

    public static void RunAll()
    {
        TR.Run("GC_Finalizer_RunsOnUnreachable", TestRunsOnUnreachable);
        TR.Run("GC_Finalizer_RunsExactlyOnce", TestRunsExactlyOnce);
        TR.Run("GC_Finalizer_NotRunWhileReachable", TestNotRunWhileReachable);
        TR.Run("GC_Finalizer_ManyObjects", TestManyObjects);
        TR.Run("GC_Finalizer_DerivedThenBase", TestDerivedThenBase);
        TR.Run("GC_Finalizer_GenericNew", TestGenericNew);
        TR.Run("GC_Finalizer_SuppressFinalize", TestSuppressFinalize);
        TR.Run("GC_Finalizer_ReRegisterAfterSuppress", TestReRegisterAfterSuppress);
        TR.Run("GC_Finalizer_ReRegisterInFinalizer", TestReRegisterInFinalizer);
        TR.Run("GC_Finalizer_GraphAliveInFinalizer", TestGraphAliveInFinalizer);
        TR.Run("GC_Finalizer_Resurrection", TestResurrection);
        TR.Run("GC_Finalizer_ShortWeakCleared", TestShortWeakCleared);
        TR.Run("GC_Finalizer_LongWeakTracksResurrection", TestLongWeakTracksResurrection);
        TR.Run("GC_Finalizer_AllocInFinalizer", TestAllocInFinalizer);
        TR.Run("GC_Finalizer_FinalizableCreatedInFinalizer", TestFinalizableCreatedInFinalizer);
        TR.Run("GC_Finalizer_CollectInFinalizer", TestCollectInFinalizer);
        TR.Run("GC_Finalizer_ThrowingFinalizer", TestThrowingFinalizer);
        TR.Run("GC_Finalizer_DeferredWhenInterruptsMasked", TestDeferredWhenInterruptsMasked);
        TR.Run("GC_Finalizer_MultithreadRegistration", TestMultithreadRegistration);
        TR.Run("GC_Finalizer_PendingCount", TestPendingCount);
        TR.Run("GC_Finalizer_SuppressQueuedFromFinalizer", TestSuppressQueuedFromFinalizer);
        TR.Run("GC_Finalizer_DependentValueAliveInFinalizer", TestDependentValueAliveInFinalizer);
        TR.Run("GC_Finalizer_DoubleRegistrationRunsTwice", TestDoubleRegistrationRunsTwice);
        TR.Run("GC_Finalizer_SuppressCancelsEveryRegistration", TestSuppressCancelsEveryRegistration);
        TR.Run("GC_Finalizer_AllocWhileMaskedDefers", TestAllocWhileMaskedDefers);
        TR.Run("GC_Finalizer_MaxGeneration", TestMaxGeneration);
    }

    // ==================== Probes ====================

    private class CountingProbe
    {
        protected readonly int[] Counter;

        public CountingProbe(int[] counter)
        {
            Counter = counter;
        }

        ~CountingProbe()
        {
            Counter[0]++;
        }
    }

    /// <summary>
    /// Records the order of the two finalizers: the derived one must run first, then the base one.
    /// </summary>
    private sealed class DerivedProbe : CountingProbe
    {
        private readonly int[] _order;

        public DerivedProbe(int[] counter, int[] order) : base(counter)
        {
            _order = order;
        }

        ~DerivedProbe()
        {
            // Counter[0] is still 0 here only if the base finalizer has not run yet.
            _order[0] = Counter[0] == 0 ? 1 : -1;
        }
    }

    private sealed class GenericProbe
    {
        ~GenericProbe()
        {
            s_genericFinalized++;
        }
    }

    private sealed class ReRegisteringProbe
    {
        private readonly int[] _counter;
        private readonly int _runs;

        public ReRegisteringProbe(int[] counter, int runs)
        {
            _counter = counter;
            _runs = runs;
        }

        ~ReRegisteringProbe()
        {
            _counter[0]++;
            if (_counter[0] < _runs)
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }

    private sealed class Node
    {
        public Node? Next;
        public int[]? Data;
    }

    /// <summary>
    /// Owns a graph nothing else references. The finalizer checks the graph was kept alive with
    /// it: a child the collection swept is a free block by then, and its pattern is gone.
    /// </summary>
    private sealed class GraphProbe
    {
        private readonly int[] _result;
        private readonly Node _head;

        public GraphProbe(int[] result, int depth)
        {
            _result = result;
            Node? head = null;
            for (int i = 0; i < depth; i++)
            {
                head = new Node { Next = head, Data = NewPatternArray() };
            }

            _head = head!;
        }

        ~GraphProbe()
        {
            int nodes = 0;
            bool intact = true;
            for (Node? node = _head; node != null; node = node.Next)
            {
                intact &= IsPatternArray(node.Data);
                nodes++;
            }

            _result[0] = intact ? nodes : -1;
        }
    }

    private sealed class ResurrectingProbe
    {
        public readonly int[] Payload = NewPatternArray();
        private readonly int[] _counter;

        public ResurrectingProbe(int[] counter)
        {
            _counter = counter;
        }

        ~ResurrectingProbe()
        {
            _counter[0]++;
            s_resurrected = this;
        }
    }

    private sealed class AllocatingProbe
    {
        private readonly int[] _result;

        public AllocatingProbe(int[] result)
        {
            _result = result;
        }

        ~AllocatingProbe()
        {
            // Enough to leave the current allocation buffer and go through a refill.
            int sum = 0;
            for (int i = 0; i < 64; i++)
            {
                byte[] buffer = new byte[512];
                buffer[i] = (byte)(i + 1);
                sum += buffer[i];
            }

            string text = "finalizer-" + sum;
            _result[0] = text.Length == 14 && sum == 2080 ? 1 : -1;
        }
    }

    private sealed class SpawningProbe
    {
        private readonly int[] _counter;
        private readonly int[] _childCounter;

        public SpawningProbe(int[] counter, int[] childCounter)
        {
            _counter = counter;
            _childCounter = childCounter;
        }

        ~SpawningProbe()
        {
            _counter[0]++;
            SpawnChild(_childCounter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void SpawnChild(int[] childCounter)
        {
            object child = new CountingProbe(childCounter);
            if (child == null)
            {
                childCounter[0] = -1;
            }
        }
    }

    private sealed class CollectingProbe
    {
        private readonly int[] _counter;

        public CollectingProbe(int[] counter)
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
        private readonly int[] _counter;

        public ThrowingProbe(int[] counter)
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

    /// <summary>
    /// Cancels the finalizer of another object that died in the same collection. Registered
    /// first, so it is queued, and runs, before its victim.
    /// </summary>
    private sealed class SuppressingProbe
    {
        public object? Victim;
        private readonly int[] _counter;

        public SuppressingProbe(int[] counter)
        {
            _counter = counter;
        }

        ~SuppressingProbe()
        {
            _counter[0]++;
            if (Victim != null)
            {
                GC.SuppressFinalize(Victim);
            }
        }
    }

    /// <summary>
    /// Looks itself up in <see cref="s_table"/> from its finalizer: the value a dependent handle
    /// hangs on it must stay alive until the finalizer has run.
    /// </summary>
    private sealed class TableProbe
    {
        private readonly int[] _result;

        public TableProbe(int[] result)
        {
            _result = result;
        }

        ~TableProbe()
        {
            ConditionalWeakTable<object, int[]>? table = s_table;
            bool found = table != null && table.TryGetValue(this, out int[]? data) && IsPatternArray(data);
            _result[0] = found ? 1 : -1;
        }
    }

    // ==================== Helpers ====================

    private static int[] NewPatternArray()
    {
        int[] data = new int[64];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Pattern + i;
        }

        return data;
    }

    private static bool IsPatternArray(int[]? data)
    {
        return data != null && data.Length == 64 && data[0] == Pattern && data[63] == Pattern + 63;
    }

    private static void Collect(int passes)
    {
        for (int i = 0; i < passes; i++)
        {
            CoreGC.Collect();
        }
    }

    /// <summary>
    /// Overwrites the memory the last collection freed, so a use of a swept object shows.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateGarbage()
    {
        for (int i = 0; i < 256; i++)
        {
            int[] garbage = new int[64];
            garbage[0] = i;
            garbage[63] = i;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateCountingProbes(int[] counter, int count)
    {
        for (int i = 0; i < count; i++)
        {
            object probe = new CountingProbe(counter);
            if (probe == null)
            {
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T CreateGeneric<T>() where T : new()
    {
        return new T();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateGenericProbe()
    {
        object probe = CreateGeneric<GenericProbe>();
        if (probe == null)
        {
            s_genericFinalized = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateDerivedProbe(int[] counter, int[] order)
    {
        object probe = new DerivedProbe(counter, order);
        if (probe == null)
        {
            order[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSuppressedProbe(int[] counter, bool reRegister)
    {
        CountingProbe probe = new CountingProbe(counter);
        GC.SuppressFinalize(probe);
        if (reRegister)
        {
            GC.ReRegisterForFinalize(probe);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateReRegisteringProbe(int[] counter, int runs)
    {
        object probe = new ReRegisteringProbe(counter, runs);
        if (probe == null)
        {
            counter[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateGraphProbe(int[] result, int depth)
    {
        object probe = new GraphProbe(result, depth);
        if (probe == null)
        {
            result[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateResurrectingProbe(int[] counter, bool trackResurrection)
    {
        return new WeakReference(new ResurrectingProbe(counter), trackResurrection);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool ResurrectedPayloadIntact()
    {
        return s_resurrected is ResurrectingProbe probe && IsPatternArray(probe.Payload);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateAllocatingProbe(int[] result)
    {
        object probe = new AllocatingProbe(result);
        if (probe == null)
        {
            result[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSpawningProbe(int[] counter, int[] childCounter)
    {
        object probe = new SpawningProbe(counter, childCounter);
        if (probe == null)
        {
            counter[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateCollectingProbe(int[] counter)
    {
        object probe = new CollectingProbe(counter);
        if (probe == null)
        {
            counter[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateThrowingProbe(int[] counter)
    {
        object probe = new ThrowingProbe(counter);
        if (probe == null)
        {
            counter[0] = -1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateSuppressingPair(int[] suppressor, int[] victim)
    {
        SuppressingProbe probe = new SuppressingProbe(suppressor);
        probe.Victim = new CountingProbe(victim);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateTableProbe(ConditionalWeakTable<object, int[]> table, int[] result)
    {
        TableProbe probe = new TableProbe(result);
        table.Add(probe, NewPatternArray());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CreateDoubleRegisteredProbe(int[] counter, bool suppress)
    {
        CountingProbe probe = new CountingProbe(counter);
        GC.ReRegisterForFinalize(probe);
        if (suppress)
        {
            GC.SuppressFinalize(probe);
        }
    }

    // ==================== Tests ====================

    private static void TestRunsOnUnreachable()
    {
        int[] counter = new int[1];
        CreateCountingProbes(counter, 1);

        Collect(1);

        Assert.Equal(1, counter[0], "Finalizer: an unreachable object is finalized by the collection that finds it dead");
    }

    private static void TestRunsExactlyOnce()
    {
        int[] counter = new int[1];
        CreateCountingProbes(counter, 1);

        Collect(1);
        AllocateGarbage();
        Collect(3);

        Assert.Equal(1, counter[0], "Finalizer: later collections must not finalize the same object again");
    }

    private static void TestNotRunWhileReachable()
    {
        int[] counter = new int[1];
        CountingProbe probe = new CountingProbe(counter);

        Collect(3);

        Assert.Equal(0, counter[0], "Finalizer: a reachable object must not be finalized");
        GC.KeepAlive(probe);
    }

    private static void TestManyObjects()
    {
        // More than the registry holds at first, so it grows while objects are being registered.
        const int count = 2000;
        int[] counter = new int[1];
        CreateCountingProbes(counter, count);

        Collect(2);

        // The conservative scan may falsely retain a few of them.
        Assert.True(counter[0] >= count - 2,
            "Finalizer: must finalize at least " + (count - 2) + " of " + count + " objects, got " + counter[0]);
        Assert.True(counter[0] <= count,
            "Finalizer: must not finalize more than the " + count + " objects created, got " + counter[0]);
    }

    private static void TestDerivedThenBase()
    {
        int[] counter = new int[1];
        int[] order = new int[1];
        CreateDerivedProbe(counter, order);

        Collect(1);

        Assert.Equal(1, order[0], "Finalizer: the derived finalizer runs, and before the base one");
        Assert.Equal(1, counter[0], "Finalizer: the base finalizer runs once after the derived one");
    }

    private static void TestGenericNew()
    {
        // new T() allocates through the runtime's allocator lookup, not the RhpNewFinalizable
        // call the compiler emits for a direct new.
        s_genericFinalized = 0;
        CreateGenericProbe();

        Collect(1);

        Assert.Equal(1, s_genericFinalized, "Finalizer: an object created through new T() is registered too");
    }

    private static void TestSuppressFinalize()
    {
        int[] counter = new int[1];
        CreateSuppressedProbe(counter, false);

        Collect(3);

        Assert.Equal(0, counter[0], "Finalizer: GC.SuppressFinalize must cancel the finalizer");
    }

    private static void TestReRegisterAfterSuppress()
    {
        int[] counter = new int[1];
        CreateSuppressedProbe(counter, true);

        Collect(3);

        Assert.Equal(1, counter[0], "Finalizer: GC.ReRegisterForFinalize restores a suppressed finalizer, once");
    }

    private static void TestReRegisterInFinalizer()
    {
        const int runs = 3;
        int[] counter = new int[1];
        CreateReRegisteringProbe(counter, runs);

        Collect(1);
        Assert.Equal(1, counter[0], "Finalizer: first run of a re-registering finalizer");

        Collect(runs + 3);
        Assert.Equal(runs, counter[0],
            "Finalizer: a finalizer that re-registers itself runs once per collection until it stops");
    }

    private static void TestGraphAliveInFinalizer()
    {
        const int depth = 8;
        int[] result = new int[1];
        CreateGraphProbe(result, depth);

        Collect(1);

        Assert.Equal(depth, result[0],
            "Finalizer: everything the object references must still be intact when its finalizer runs");
    }

    private static void TestResurrection()
    {
        s_resurrected = null;
        int[] counter = new int[1];
        WeakReference tracking = CreateResurrectingProbe(counter, true);

        Collect(1);
        Assert.Equal(1, counter[0], "Finalizer: the resurrecting finalizer ran");
        Assert.True(s_resurrected != null, "Finalizer: the finalizer stored the object in a static");

        // Now reachable again: it must survive like any other object, and not be finalized again.
        AllocateGarbage();
        Collect(2);
        AllocateGarbage();
        Assert.True(ResurrectedPayloadIntact(), "Finalizer: a resurrected object and its fields survive later collections");
        Assert.Equal(1, counter[0], "Finalizer: a resurrected object is not finalized a second time");

        // Dropped for good: no second finalization, and this time the memory goes.
        s_resurrected = null;
        Collect(2);
        Assert.Equal(1, counter[0], "Finalizer: dropping a resurrected object does not finalize it again");
        Assert.True(tracking.Target == null, "Finalizer: a resurrected object is reclaimed once it is dropped");
    }

    private static void TestShortWeakCleared()
    {
        s_resurrected = null;
        int[] counter = new int[1];
        WeakReference shortRef = CreateResurrectingProbe(counter, false);

        Collect(1);

        Assert.Equal(1, counter[0], "Finalizer: the finalizer ran");
        Assert.True(shortRef.Target == null,
            "Finalizer: a short weak reference is cleared when its target is queued for finalization, resurrected or not");

        s_resurrected = null;
        Collect(2);
    }

    private static void TestLongWeakTracksResurrection()
    {
        s_resurrected = null;
        int[] counter = new int[1];
        WeakReference longRef = CreateResurrectingProbe(counter, true);

        Collect(1);

        Assert.Equal(1, counter[0], "Finalizer: the finalizer ran");
        Assert.True(longRef.Target != null,
            "Finalizer: a weak reference tracking resurrection keeps its target past finalization");
        Assert.True(ReferenceEquals(longRef.Target, s_resurrected),
            "Finalizer: the tracking weak reference still points at the resurrected object");

        s_resurrected = null;
        Collect(2);
        Assert.True(longRef.Target == null, "Finalizer: the tracking weak reference is cleared once the object is gone");
    }

    private static void TestAllocInFinalizer()
    {
        int[] result = new int[1];
        CreateAllocatingProbe(result);

        Collect(1);
        Assert.Equal(1, result[0], "Finalizer: a finalizer may allocate, and reads back what it allocated");

        // The heap must still be usable and consistent afterwards.
        AllocateGarbage();
        Collect(1);
        Assert.True(IsPatternArray(NewPatternArray()), "Finalizer: allocation works after a finalizer allocated");
    }

    private static void TestFinalizableCreatedInFinalizer()
    {
        int[] counter = new int[1];
        int[] childCounter = new int[1];
        CreateSpawningProbe(counter, childCounter);

        Collect(1);
        Assert.Equal(1, counter[0], "Finalizer: the spawning finalizer ran");

        Collect(3);
        Assert.Equal(1, counter[0], "Finalizer: the spawning finalizer ran once");
        Assert.Equal(1, childCounter[0],
            "Finalizer: an object created by a finalizer is registered and finalized by a later collection");
    }

    private static void TestCollectInFinalizer()
    {
        int[] counter = new int[2];
        CreateCollectingProbe(counter);

        Collect(2);

        Assert.Equal(1, counter[0], "Finalizer: a finalizer calling GC.Collect runs once");
        Assert.Equal(1, counter[1], "Finalizer: GC.Collect returns inside a finalizer");

        CoreGC.GetStats(out int before, out int _);
        Collect(1);
        CoreGC.GetStats(out int after, out int _);
        Assert.Equal(before + 1, after, "Finalizer: collections still run after a finalizer called GC.Collect");
    }

    private static void TestThrowingFinalizer()
    {
        // The kernel has no process to tear down: a finalizer that throws is contained, and the
        // objects queued beside it are still finalized.
        int[] counter = new int[1];
        int[] throwing = new int[1];
        CreateCountingProbes(counter, 2);
        CreateThrowingProbe(throwing);
        CreateCountingProbes(counter, 2);

        Collect(2);

        Assert.Equal(1, throwing[0], "Finalizer: the throwing finalizer ran once");
        Assert.Equal(4, counter[0], "Finalizer: a throwing finalizer does not stop the others");

        CreateCountingProbes(counter, 1);
        Collect(1);
        Assert.Equal(5, counter[0], "Finalizer: finalization keeps working after a finalizer threw");
    }

    private static void TestDeferredWhenInterruptsMasked()
    {
        int[] counter = new int[1];
        CreateCountingProbes(counter, 1);

        // Heap.Collect masks interrupts around the collection, as the allocator does when it
        // collects: the finalizer is queued, not run in the masked section.
        Heap.Collect();
        int afterMaskedCollect = counter[0];

        GC.WaitForPendingFinalizers();

        Assert.Equal(0, afterMaskedCollect, "Finalizer: no finalizer runs while interrupts are masked");
        Assert.Equal(1, counter[0], "Finalizer: GC.WaitForPendingFinalizers runs what a masked collection queued");
    }

    private static void RegistrationWorker()
    {
        for (int i = 0; i < 3000; i++)
        {
            object probe = new WorkerProbe();
            if (probe != null)
            {
                Interlocked.Increment(ref s_workerAllocated);
            }
        }

        Interlocked.Increment(ref s_workerDone);
    }

    private static void TestMultithreadRegistration()
    {
        s_workerAllocated = 0;
        s_workerFinalized = 0;
        s_workerDone = 0;

        Thread worker1 = new Thread(RegistrationWorker);
        Thread worker2 = new Thread(RegistrationWorker);
        worker1.Start();
        worker2.Start();

        // Collect, and so finalize, while the workers register new objects.
        for (int i = 0; i < 400 && s_workerDone < 2; i++)
        {
            CoreGC.Collect();
            TimerManager.Wait(5);
        }

        Assert.Equal(2, s_workerDone, "Finalizer: both registration workers must finish");

        Collect(3);

        int allocated = s_workerAllocated;
        int finalized = s_workerFinalized;
        Assert.Equal(6000, allocated, "Finalizer: every worker allocation succeeded");
        Assert.True(finalized <= allocated,
            "Finalizer: no object finalized twice under concurrent registration (" + finalized + " of " + allocated + ")");
        Assert.True(finalized >= allocated - 8,
            "Finalizer: no registration lost under concurrent registration (" + finalized + " of " + allocated + ")");
    }

    private static void TestPendingCount()
    {
        const int count = 10;

        // CoreLib's Gen2GcCallback re-registers itself from its finalizer to be called back after
        // every collection: each one finds it dead again. Measured here, once the probes earlier
        // tests left behind are finalized.
        Collect(2);
        GC.WaitForPendingFinalizers();
        Collect(1);
        int baseline = CoreGC.GetFinalizationPendingCount();
        GC.WaitForPendingFinalizers();

        int[] counter = new int[1];
        CreateCountingProbes(counter, count);

        // Masked: the probes are queued, not run, so the collection ends with them pending.
        Heap.Collect();
        int pending = CoreGC.GetFinalizationPendingCount();
        int ranInMaskedCollect = counter[0];

        GC.WaitForPendingFinalizers();
        long reported = GC.GetGCMemoryInfo().FinalizationPendingCount;

        Assert.Equal(0, ranInMaskedCollect, "Finalizer: the masked collection left its finalizers pending");
        Assert.True(pending >= baseline + count - 2,
            "Finalizer: the pending count covers the queued objects, expected at least " + (baseline + count - 2) + ", got " + pending);
        Assert.Equal((long)pending, reported, "Finalizer: GCMemoryInfo.FinalizationPendingCount reports the collector's count");

        // The probes the conservative scan kept die in a later collection: finalize them first.
        Collect(2);
        GC.WaitForPendingFinalizers();

        // No probe died: the next collection ends with the baseline alone pending.
        Collect(1);
        int leftover = CoreGC.GetFinalizationPendingCount();
        Assert.Equal(baseline, leftover,
            "Finalizer: no probe pending after a collection that found none dead, got " + leftover + " for a baseline of " + baseline);
    }

    private static void TestSuppressQueuedFromFinalizer()
    {
        // Both die in the same collection; the first one's finalizer cancels the second's while
        // it waits in the queue.
        int[] suppressor = new int[1];
        int[] victim = new int[1];
        CreateSuppressingPair(suppressor, victim);

        Collect(1);
        Collect(2);

        Assert.Equal(1, suppressor[0], "Finalizer: the suppressing finalizer ran");
        Assert.Equal(0, victim[0], "Finalizer: GC.SuppressFinalize cancels an object already queued for finalization");
    }

    private static void TestDependentValueAliveInFinalizer()
    {
        int[] result = new int[1];
        ConditionalWeakTable<object, int[]> table = new ConditionalWeakTable<object, int[]>();
        s_table = table;
        CreateTableProbe(table, result);

        Collect(1);
        AllocateGarbage();
        Collect(1);

        Assert.Equal(1, result[0],
            "Finalizer: a ConditionalWeakTable value hung on an object is still there when its finalizer runs");

        s_table = null;
        GC.KeepAlive(table);
    }

    private static void TestDoubleRegistrationRunsTwice()
    {
        // As in .NET: ReRegisterForFinalize on an object still registered adds a second run.
        int[] counter = new int[1];
        CreateDoubleRegisteredProbe(counter, false);

        Collect(3);

        Assert.Equal(2, counter[0], "Finalizer: an object registered twice is finalized twice");
    }

    private static void TestSuppressCancelsEveryRegistration()
    {
        // .NET marks the object, so SuppressFinalize cancels the finalizer however many times it
        // was registered, until the next ReRegisterForFinalize.
        int[] counter = new int[1];
        CreateDoubleRegisteredProbe(counter, true);

        Collect(3);

        Assert.Equal(0, counter[0], "Finalizer: GC.SuppressFinalize cancels every pending registration of an object");
    }

    private static void TestAllocWhileMaskedDefers()
    {
        // An allocation made with interrupts masked (an IRQ handler, a spin-locked section) must
        // neither run the queued finalizers nor block: it leaves them to a later call.
        int[] counter = new int[1];
        CreateCountingProbes(counter, 1);

        int ranInside;
        bool allocated;
        using (InternalCpu.DisableInterruptsScope())
        {
            CoreGC.Collect();
            object fresh = new object();
            allocated = fresh != null;
            ranInside = counter[0];
        }

        GC.WaitForPendingFinalizers();

        Assert.True(allocated, "Finalizer: allocating with interrupts masked and finalizers pending succeeds");
        Assert.Equal(0, ranInside, "Finalizer: no finalizer runs from an allocation made with interrupts masked");
        Assert.Equal(1, counter[0], "Finalizer: the finalizer runs once interrupts are back");
    }

    private static void TestMaxGeneration()
    {
        Assert.Equal(0, GC.MaxGeneration, "Finalizer: the collector has a single generation");
    }
}
