// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Monitor = Cosmos.Kernel.Core.Scheduler.Monitor;

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

    /// <summary>
    /// State of the finalizer thread. Do not use it outside of GargageColection.Finalizer.
    /// If you are using it you should now what your doing.
    /// </summary>
    private enum ThreadFinalizerStatus
    {
        Stopped,
        Starting,
        Running,
        Stopping,
        Exited,
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

    /// <summary>
    /// Monitor for <see cref="s_finalizerThread"/> thread.
    /// Lazy init by <see cref="ThreadedFinalizer"/>.
    /// Use to protect <see cref="s_finalizerRequested"/>,
    /// <see cref="s_finalizerRequestSeq"/> and <see cref="s_finalizerDoneSeq"/>.
    /// </summary>
    private static Monitor? s_finalizerMonitor = null;

    private static bool s_finalizerRequested;       // protected by s_finalizerMonitor
    private static ulong s_finalizerRequestSeq;     // protected by s_finalizerMonitor
    private static ulong s_finalizerDoneSeq;        // protected by s_finalizerMonitor

    /// <summary>
    /// Use <see cref="s_finalizerMonitor"/> to signal a major state.
    /// Like <see cref="ThreadFinalizerStatus.Stopped"/> and <see cref="ThreadFinalizerStatus.Running"/>.
    /// Use <see cref="Interlocked"/> to interact with it.
    /// </summary>
    private static volatile ThreadFinalizerStatus s_threadFinalizerStatus = ThreadFinalizerStatus.Stopped;

    // --- Properties ---

    /// <summary>
    /// Set to <see langword="true"/> to run instance waiting for finaliazer on a side thread.
    /// Getting <see langword="false"/> indicate that the thread has not started.
    /// Starting or stopping the thread will be reported as the thread not started.
    /// Edge case: The thread is stopping and you set to true. It will not start the thread.
    /// </summary>
    [MemberNotNullWhen(true, "s_finalizerMonitor")] // lazy init, also true after
    [MemberNotNullWhen(true, "s_finalizerThread")]
    public static bool ThreadedFinalizer
    {
        get => s_threadFinalizerStatus is ThreadFinalizerStatus.Running or ThreadFinalizerStatus.Starting;
        set
        {
            if (!CosmosFeatures.SchedulerEnabled)
            {
                return;
            }
            if (value)
            {
                StartFinalizerThread();
            }
            else
            {
                StopFinalizerThread();
            }
        }
    }

    // --- Public methods ---

    /// <summary>
    /// Returns the number of objects that were waiting for their finalizer when the last
    /// collection ended.
    /// </summary>
    /// <returns>The count recorded by the last collection.</returns>
    public static int GetFinalizationPendingCount()
    {
        return s_lastFinalizationPendingCount;
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
    /// <remarks>
    /// As in .NET, every registration of the object is cancelled, however many times it was
    /// registered, until the next <see cref="TryRegisterForFinalize"/>.
    /// </remarks>
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
                }
            }

            // Queued by a collection and revived since, or suppressed from another finalizer.
            for (int i = s_finalizerQueueHead; i < s_finalizerQueue.Count; i++)
            {
                if (s_finalizerQueue.Items[i] == obj)
                {
                    s_finalizerQueue.Items[i] = null;
                }
            }
        }
    }

    /// <summary>
    /// Run a pass with the finalizer thread if <see cref="ThreadedFinalizer"/> is <see langword="true"/>
    /// Else use the current thread to execute the finalizers.
    /// </summary>
    internal static void WaitPendingFinalizers()
    {
        // Masked (an IRQ handler, a spin-locked section): the monitor would block a CPU nothing
        // preempts any more, and a finalizer is not to run there either
        if (!InternalCpu.InterruptsEnabled
            || !FinalizationPending()
            // if the finalizer try to wait it's self (avoid deadlock)
            || ReferenceEquals(SchedulerManager.CurrentCpuState?.CurrentThread, s_finalizerThread))
        {
            return;
        }

        // Running, not Starting: the starter holds the monitor, and may not have created it yet
        if (s_threadFinalizerStatus == ThreadFinalizerStatus.Running)
        {
            Monitor monitor = s_finalizerMonitor!;

            monitor.Acquire();
            // uncheck in case of an overflow after ~500 year it reset
            ulong ticket = unchecked(++s_finalizerRequestSeq);
            s_finalizerRequested = true;
            monitor.SignalAll();

            monitor.Acquire();
            while (!IsPassDone(s_finalizerDoneSeq, ticket) && ThreadedFinalizer)
            {
                monitor.Wait();
            }
            bool done = IsPassDone(s_finalizerDoneSeq, ticket);
            monitor.Release();

            if (!done)
            {
                RunPendingFinalizers();
            }
        }
        else
        {
            RunPendingFinalizers();
        }

        static bool IsPassDone(ulong done, ulong ticket) => unchecked((long)(done - ticket)) >= 0;
    }

    /// <summary>
    /// Run the finalizers on the calling thread if <see cref="ThreadedFinalizer"/> is <see langword="false"/>.
    /// Else signal the thread in charge to finalize all instances, without waiting for it.
    /// </summary>
    /// <remarks>
    /// Does nothing when interrupts are masked: the queue is left to the next allocation.
    /// </remarks>
    internal static void SignalPendingFinalizers()
    {
        if (!InternalCpu.InterruptsEnabled)
        {
            return;
        }

        if (s_threadFinalizerStatus == ThreadFinalizerStatus.Running)
        {
            // Signalled already, or in a pass, which drains the queue to its end, what a
            // collection queues meanwhile included (a finalizer that collects lands here too)
            if (s_finalizerRequested || s_runningFinalizers)
            {
                return;
            }

            Monitor monitor = s_finalizerMonitor!;
            monitor.Acquire();
            s_finalizerRequested = true;
            monitor.SignalAll();
        }
        else
        {
            RunPendingFinalizers();
        }
    }

    // --- Private methods ---

    private static void RunFinalizerWorker()
    {
        Monitor monitor = s_finalizerMonitor!;
        monitor.Acquire();

        while (!s_finalizerRequested && ThreadedFinalizer)
        {
            monitor.Wait();
        }

        while (ThreadedFinalizer)
        {
            s_finalizerRequested = false;
            ulong seq = s_finalizerRequestSeq;
            monitor.Release();

            RunPendingFinalizers();

            monitor.Acquire();
            s_finalizerDoneSeq = seq;
            monitor.SignalAll();
            monitor.Acquire();

            while (!s_finalizerRequested && ThreadedFinalizer)
            {
                monitor.Wait();
            }
        }

        s_threadFinalizerStatus = ThreadFinalizerStatus.Exited;
        monitor.SignalAll();
    }

    /// <summary>
    /// DO NOT CALL THIS METHOD use <see cref="ThreadedFinalizer"/>.
    /// It's an helper method for a cleaner code.
    /// </summary>
    private static void StartFinalizerThread()
    {
        // This enum do verrrrry long code but i did some mistak when i use an int.
        ThreadFinalizerStatus prev = Interlocked.CompareExchange(ref s_threadFinalizerStatus,
                                                                 ThreadFinalizerStatus.Starting, ThreadFinalizerStatus.Stopped);
        if (prev is ThreadFinalizerStatus.Starting or ThreadFinalizerStatus.Running)
        {
            return;
        }

        if (prev != ThreadFinalizerStatus.Stopped)
        {
            // Stopping / Exited: wait for the stopper to publish Stopped, then claim it.
            // if we are here some one init the lazy beffor use
            Monitor m = s_finalizerMonitor!;
            m.Acquire();
            while (true)
            {
                prev = Interlocked.CompareExchange(ref s_threadFinalizerStatus,
                                                   ThreadFinalizerStatus.Starting, ThreadFinalizerStatus.Stopped);
                if (prev == ThreadFinalizerStatus.Stopped)
                {
                    break;
                }
                if (prev is ThreadFinalizerStatus.Starting or ThreadFinalizerStatus.Running)
                {
                    // if some one else was waiting to restart
                    m.Release();
                    return;
                }
                m.Wait();
            }
            m.Release();
        }

        // Only the CAS winner reaches here, so the lazy ??= is not racy.
        s_finalizerMonitor ??= new Monitor();
        Monitor monitor = s_finalizerMonitor;

        monitor.Acquire();
        s_finalizerRequested = false;
        s_finalizerDoneSeq = s_finalizerRequestSeq;

        if (KernelThread.TryStart(RunFinalizerWorker, out s_finalizerThread))
        {
            s_threadFinalizerStatus = ThreadFinalizerStatus.Running;
        }
        else
        {
            // TODO: report the error
            s_threadFinalizerStatus = ThreadFinalizerStatus.Stopped;
        }
        monitor.SignalAll();
    }

    /// <summary>
    /// DO NOT CALL THIS METHOD use <see cref="ThreadedFinalizer"/>
    /// It's an helper method for a cleaner code.
    /// </summary>
    private static void StopFinalizerThread()
    {
        Monitor? monitor = s_finalizerMonitor;
        if (monitor is null)
        {
            return;
        }

        monitor.Acquire();

        while (s_threadFinalizerStatus == ThreadFinalizerStatus.Starting)
        {
            monitor.Wait();
        }

        if (s_threadFinalizerStatus != ThreadFinalizerStatus.Running)
        {
            // already Stopping/Exited/Stopped
            monitor.Release();
            return;
        }

        s_threadFinalizerStatus = ThreadFinalizerStatus.Stopping;
        monitor.SignalAll();

        monitor.Acquire();
        while (s_threadFinalizerStatus != ThreadFinalizerStatus.Exited)
        {
            monitor.Wait();
        }

        s_finalizerThread = null;
        s_threadFinalizerStatus = ThreadFinalizerStatus.Stopped;
        monitor.SignalAll();
    }

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
    // TODO: RunPendingFinalizers is not thread safe! For now not big deal only one core on Cosmos
    // InternalCpu.DisableInterruptsScope() made it thread safe (for now)
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
