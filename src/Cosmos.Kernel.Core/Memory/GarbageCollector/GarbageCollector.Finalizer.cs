// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Mutex = Cosmos.Kernel.Core.Scheduler.Mutex;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Finalization: the registry of the objects that have a finalizer to run, the queue of those a
/// collection found dead, and the pass that runs their finalizers.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    // --- Nested types ---

    /// <summary>
    /// A growable array of object pointers in pages of its own. The collector does not scan
    /// those pages, so an entry keeps nothing alive by itself, and adding one never allocates
    /// on the GC heap: it is filled from inside the allocator and from inside a collection.
    /// </summary>
    private struct FinalizerList
    {
        public GCObject** Items;

        public int Count;

        private int _capacity;

        /// <summary>
        /// Makes room for <paramref name="extra"/> more entries.
        /// </summary>
        /// <param name="extra">Number of entries about to be added.</param>
        /// <returns><c>false</c> when the page allocator has no room for the larger array.</returns>
        public bool EnsureRoom(int extra)
        {
            int needed = Count + extra;
            if (needed <= _capacity)
            {
                return true;
            }

            int newCapacity = _capacity * 2;
            if (newCapacity < needed)
            {
                newCapacity = needed;
            }

            ulong pageCount = ((ulong)newCapacity * (ulong)sizeof(GCObject*) + PageAllocator.PageSize - 1) / PageAllocator.PageSize;
            GCObject** newItems = (GCObject**)PageAllocator.AllocPages(PageType.Unmanaged, pageCount, false);
            if (newItems == null)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                newItems[i] = Items[i];
            }

            if (Items != null)
            {
                PageAllocator.Free(Items);
            }

            Items = newItems;
            _capacity = (int)(pageCount * PageAllocator.PageSize / (ulong)sizeof(GCObject*));
            return true;
        }

        /// <summary>
        /// Appends an entry.
        /// </summary>
        /// <param name="obj">The object to add.</param>
        /// <returns><c>false</c> when the array is full and cannot grow.</returns>
        public bool Add(GCObject* obj)
        {
            if (!EnsureRoom(1))
            {
                return false;
            }

            Items[Count++] = obj;
            return true;
        }
    }

    // --- Static fields ---

    /// <summary>
    /// The live objects whose finalizer is still to run. Not a root: an entry no root reaches is
    /// what a collection moves to <see cref="s_finalizerQueue"/>.
    /// </summary>
    private static FinalizerList s_finalizable;

    /// <summary>
    /// The objects a collection found dead, waiting for their finalizer. A root: an entry and
    /// everything it references stay alive until its finalizer has run. Entries before
    /// <see cref="s_finalizerQueueHead"/> are done; a null entry had its finalizer suppressed.
    /// </summary>
    private static FinalizerList s_finalizerQueue;

    /// <summary>
    /// Index of the next entry of <see cref="s_finalizerQueue"/> to finalize.
    /// </summary>
    private static int s_finalizerQueueHead;

    /// <summary>
    /// Whether a thread is inside <see cref="RunPendingFinalizers"/>. A collection a finalizer
    /// triggers, and any other thread, leave the queue to that thread.
    /// </summary>
    private static bool s_runningFinalizers;

    /// <summary>
    /// Number of objects waiting for their finalizer when the last collection ended.
    /// </summary>
    private static int s_lastFinalizationPendingCount;

    /// <summary>
    /// Thread use to rune the finalize instances.
    /// </summary>
    private static SchedulerThread? s_finalizerThread;

    // state machine of the finilize, we can't use a Mutex here. Mutex imply to have an started GC
    private const int FinalizerStopped = 0, FinalizerStarting = 1, FinalizerRunning = 2, FinalizerStopping = 3;

    private static InterruptEvent? s_finalizerWake = null;
    private static InterruptEvent? s_finalizerExited = null;
    private static InterruptEvent? s_finalizerWorkDone = null;

    private static volatile bool s_stopRequested;
    private static volatile int s_finalizerThreadState;

    private static Mutex? s_finalizerMutex = null;
    private static bool s_finalizerRequested;       // protected by s_finalizerMutex
    private static int s_waitPendingFinalizers;     // protected by s_finalizerMutex
    private static bool s_finalizerThreadWorking;   // protected by s_finalizerMutex

    // --- Properties ---

    /// <summary>
    /// Set to <see langword="true"/> to run instance waiting for finaliazer on a side thread.
    /// Getting <see langword="false"/> indicate that the thread has not started.
    /// Starting or stopping the thread will be reported as the thread not started.
    /// Edge case: The thread is stopping and you set to true. It will not start the thread.
    /// </summary>
    [MemberNotNullWhen(true, "s_finalizerThreadState")]
    [MemberNotNullWhen(true, "s_finalizerWake")]
    [MemberNotNullWhen(true, "s_finalizerExited")]
    [MemberNotNullWhen(true, "s_finalizerWorkDone")]
    [MemberNotNullWhen(true, "s_finalizerMutex")]
    public static bool ThreadedFinalizer
    {
        get => s_finalizerThreadState is FinalizerRunning;
        set
        {
            if (!CosmosFeatures.SchedulerEnabled)
            {
                return;
            }
            
            if (value)
            {
                if (Interlocked.CompareExchange(ref s_finalizerThreadState, FinalizerStarting, FinalizerStopped) != FinalizerStopped)
                {
                    // todo the edge case, wait the thread end to restart it...
                    return;
                }

                s_stopRequested = false;
                s_finalizerWake = new InterruptEvent();
                s_finalizerExited = new InterruptEvent();
                s_finalizerWorkDone = new InterruptEvent();
                s_finalizerMutex ??= new Mutex(); // lazy init can't init beffor GC inited
                s_waitPendingFinalizers = 0;

                if (KernelThread.TryStart(RunFinalizerWorker, out s_finalizerThread))
                {
                    s_finalizerThreadState = FinalizerRunning;
                }
                else
                {
                    // TODO: error, report it?
                    s_finalizerThreadState = FinalizerStopped;
                }
            }
            else
            {
                if (Interlocked.CompareExchange(ref s_finalizerThreadState, FinalizerStopping, FinalizerRunning) != FinalizerRunning)
                {
                    return;
                }
                s_stopRequested = true;
                s_finalizerWake!.Signal();
                s_finalizerExited!.Wait();
                s_finalizerThread = null;

                s_finalizerMutex!.Acquire();
                while (s_waitPendingFinalizers > 0)
                {
                    s_finalizerWorkDone!.Signal();
                    s_waitPendingFinalizers--;
                }
                s_finalizerThreadWorking = false;
                s_finalizerThreadState = FinalizerStopped;
                s_finalizerMutex.Release();
            }

        }
    }
    // --- Internal methods ---

    /// <summary>
    /// Registers an object for finalization. Does nothing for a type without a finalizer.
    /// </summary>
    /// <remarks>
    /// As in .NET, registering an object that is registered already makes its finalizer run
    /// once per registration.
    /// </remarks>
    /// <param name="obj">The object to register.</param>
    /// <returns><see langword="false"/> when the registry is full and cannot grow.</returns>
    internal static bool TryRegisterForFinalize(GCObject* obj)
    {
        if (obj == null || !obj->GetMethodTable()->IsFinalizable)
        {
            return true;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            if (!s_finalizable.Add(obj))
            {
                Serial.WriteString("[GC] WARNING: finalizer registry full, object will not be finalized\n");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Cancels the finalizer of an object, whether it is still alive or queued already.
    /// </summary>
    /// <param name="obj">The object whose finalizer must not run.</param>
    internal static void SuppressFinalize(GCObject* obj)
    {
        if (obj == null || !obj->GetMethodTable()->IsFinalizable)
        {
            return;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            // From the end: what is disposed is most often what was created last.
            for (int i = s_finalizable.Count - 1; i >= 0; i--)
            {
                if (s_finalizable.Items[i] == obj)
                {
                    s_finalizable.Items[i] = s_finalizable.Items[--s_finalizable.Count];
                    return;
                }
            }

            // Queued by a collection and revived since, or suppressed from another finalizer.
            for (int i = s_finalizerQueueHead; i < s_finalizerQueue.Count; i++)
            {
                if (s_finalizerQueue.Items[i] == obj)
                {
                    s_finalizerQueue.Items[i] = null;
                    return;
                }
            }
        }
    }

    internal static void WaitPendingFinalizers()
    {
        if (!FinalizationPending()
            // if the finalizer try to wait it's self (avoid deadlock)
            || ReferenceEquals(SchedulerManager.CurrentCpuState?.CurrentThread, s_finalizerThread))
        {
            return;
        }

        if (ThreadedFinalizer)
        {
            // TODO: set the thread as waiting beffor editing values
            // else the thread can be kill (from what i have read)
            s_finalizerMutex.Acquire();
            s_finalizerRequested = true;
            s_waitPendingFinalizers++;
            if (!s_finalizerThreadWorking)
            {
                s_finalizerThreadWorking = true;
                s_finalizerWake.Signal();
            }
            s_finalizerMutex.Release();

            s_finalizerWorkDone!.Wait();
        }
        else
        {
            RunPendingFinalizers();
        }
    }

    /// <summary>
    /// Run the finalizers on the calling thread if <see cref="ThreadedFinalizer"/> is <see langword="false"/>.
    /// Else signal the thread in charge to finalize all instances.
    /// </summary>
    internal static void SignalPendingFinalizers()
    {
        if (ThreadedFinalizer)
        {
            s_finalizerMutex.Acquire();
            s_finalizerRequested = true;
            if (!s_finalizerThreadWorking)
            {
                s_finalizerThreadWorking = true;
                s_finalizerWake.Signal();
            }
            s_finalizerMutex.Release();
        }
        else
        {
            RunPendingFinalizers();
        }
    }

    /// <summary>
    /// Returns the number of objects that were waiting for their finalizer when the last
    /// collection ended.
    /// </summary>
    /// <returns>The count recorded by the last collection.</returns>
    public static int GetFinalizationPendingCount()
    {
        return s_lastFinalizationPendingCount;
    }

    // --- Private methods ---

    private static bool FinalizationPending()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            return s_finalizerQueueHead < s_finalizerQueue.Count || s_runningFinalizers;
        }
    }

    /// <summary>
    /// Runs the finalizers of the objects the collections queued, on the calling thread.
    /// </summary>
    /// <remarks>
    /// Returns at once when interrupts are masked: a finalizer is arbitrary code, not to be run
    /// from an interrupt handler or a critical section. The queue is then left for the next
    /// call; the allocator makes one before each allocation.
    /// </remarks>
    private static void RunPendingFinalizers()
    {
        if (s_finalizerQueueHead == s_finalizerQueue.Count || !InternalCpu.InterruptsEnabled)
        {
            return;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            if (s_runningFinalizers)
            {
                return;
            }

            s_runningFinalizers = true;
        }

        object? target = DequeueFinalizable(out IntPtr finalizer);

        while (target != null)
        {
            try
            {
                ((delegate*<object, void>)finalizer)(target);
            }
            catch (Exception)
            {
                // TODO, open question
                Serial.WriteString("[GC] WARNING: unhandled exception in a finalizer\n");
            }
            target = DequeueFinalizable(out finalizer);
        }
    }

    private static void RunFinalizerWorker()
    {
        InterruptEvent wake = s_finalizerWake!;
        InterruptEvent exit = s_finalizerExited!;
        InterruptEvent workDone = s_finalizerWorkDone!;

        wake.Wait();
        while (!s_stopRequested)
        {
            int served;
            s_finalizerMutex!.Acquire();
            s_finalizerRequested = false;
            served = s_waitPendingFinalizers;
            s_waitPendingFinalizers = 0;
            s_finalizerMutex.Release();

            RunPendingFinalizers();

            for (int i = 0; i < served; i++)
            {
                workDone.Signal();
            }

            s_finalizerMutex.Acquire();
            if (s_finalizerRequested) 
            {
                s_finalizerMutex.Release();
                continue;
            }
            s_finalizerThreadWorking = false;
            s_finalizerMutex.Release();

            wake.Wait();
        }
        exit.Signal();
    }

    /// <summary>
    /// Takes the next object to finalize off the queue. An empty queue ends the pass: the
    /// running flag is released in the same step, so an object queued right after is not left
    /// behind by a pass that had already seen the queue empty.
    /// </summary>
    /// <param name="finalizer">Address of the finalizer of the object's type.</param>
    /// <returns>The object, now held by the caller's reference, or <c>null</c> when none is left.</returns>
    private static object? DequeueFinalizable(out IntPtr finalizer)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            while (s_finalizerQueueHead < s_finalizerQueue.Count)
            {
                GCObject* entry = s_finalizerQueue.Items[s_finalizerQueueHead++];
                if (entry != null)
                {
                    finalizer = entry->GetMethodTable()->FinalizerCode;
                    nint obj = (nint)entry;
                    return Unsafe.As<nint, object>(ref obj);
                }
            }

            s_finalizerQueueHead = 0;
            s_finalizerQueue.Count = 0;
            s_runningFinalizers = false;
            finalizer = IntPtr.Zero;
            return null;
        }
    }

    /// <summary>
    /// Marks the objects waiting for their finalizer, and what they reference. Part of the root
    /// scan.
    /// </summary>
    private static void MarkFinalizerQueue()
    {
        for (int i = s_finalizerQueueHead; i < s_finalizerQueue.Count; i++)
        {
            TryMarkRoot((nint)s_finalizerQueue.Items[i]);
        }
    }

    /// <summary>
    /// Moves the registered objects no root reached to the finalizer queue, then marks them and
    /// what they reference: they stay alive until their finalizer has run. Runs once the roots
    /// are marked and the short weak handles are cleared.
    /// </summary>
    /// <remarks>
    /// Every dead object is queued before any is marked, as in .NET: one that only another dead
    /// finalizable object references is finalized by this collection too, not kept for a later one.
    /// </remarks>
    private static void MarkFinalizableObjects()
    {
        int dead = 0;
        for (int i = 0; i < s_finalizable.Count; i++)
        {
            if (!s_finalizable.Items[i]->IsMarked)
            {
                dead++;
            }
        }

        if (dead != 0)
        {
            if (s_finalizerQueue.EnsureRoom(dead))
            {
                int kept = 0;
                for (int i = 0; i < s_finalizable.Count; i++)
                {
                    GCObject* obj = s_finalizable.Items[i];
                    if (obj->IsMarked)
                    {
                        s_finalizable.Items[kept++] = obj;
                    }
                    else
                    {
                        s_finalizerQueue.Items[s_finalizerQueue.Count++] = obj;
                    }
                }

                s_finalizable.Count = kept;
                MarkFinalizerQueue();
            }
            else
            {
                // No room to queue them: kept alive and registered, the next collection retries.
                Serial.WriteString("[GC] WARNING: finalizer queue cannot grow, finalization postponed\n");
                for (int i = 0; i < s_finalizable.Count; i++)
                {
                    TryMarkRoot((nint)s_finalizable.Items[i]);
                }
            }
        }

        s_lastFinalizationPendingCount = s_finalizerQueue.Count - s_finalizerQueueHead;
    }
}
