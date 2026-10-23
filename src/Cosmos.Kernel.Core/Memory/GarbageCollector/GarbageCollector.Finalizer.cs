// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

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
    /// State of the finalizer thread. <see cref="StartFinalizerThread"/> moves it from Stopped to
    /// Starting, then Running; <see cref="StopFinalizerThread"/> moves it to Stopping, the thread
    /// to Exited once it left its loop, and the stopper back to Stopped.
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
    /// Bit of the runtime object header that marks an object's finalizer as suppressed. The
    /// header is the 4 bytes before the object (objRef - 4), where CoreLib keeps the identity
    /// hash code or the thin lock; its <c>ObjectHeader</c> reserves bits 29 to 31 for the
    /// collector and its compare-exchange loops leave them as they are. .NET's
    /// <c>BIT_SBLK_FINALIZER_RUN</c>.
    /// </summary>
    private const int FinalizerSuppressedBit = 0x40000000;

    /// <summary>
    /// The live objects whose finalizer is still to run. Not a root: an entry no root reaches is
    /// what a collection moves to <see cref="s_finalizerQueue"/>.
    /// </summary>
    private static FinalizerList s_finalizable;

    /// <summary>
    /// The objects a collection found dead, waiting for their finalizer. A root: an entry and
    /// everything it references stay alive until its finalizer has run. Entries before
    /// <see cref="s_finalizerQueueHead"/> are done; an entry whose finalizer was suppressed
    /// meanwhile is skipped when its turn comes.
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
    /// The thread that runs the finalizers while <see cref="ThreadedFinalizer"/> is on.
    /// </summary>
    private static SchedulerThread? s_finalizerThread;

    /// <summary>
    /// Monitor of the finalizer thread, created by its first start and kept afterwards. Protects
    /// <see cref="s_finalizerRequested"/>, <see cref="s_finalizerRequestSeq"/> and
    /// <see cref="s_finalizerDoneSeq"/>, and signals the changes of
    /// <see cref="s_threadFinalizerStatus"/>.
    /// </summary>
    private static Monitor? s_finalizerMonitor;

    /// <summary>
    /// Whether a pass was asked of the finalizer thread since its last one began.
    /// </summary>
    private static bool s_finalizerRequested;

    /// <summary>
    /// Number of passes asked of the finalizer thread; a waiter takes the next one as its ticket.
    /// </summary>
    private static ulong s_finalizerRequestSeq;

    /// <summary>
    /// The request number the finalizer thread's last finished pass covered.
    /// </summary>
    private static ulong s_finalizerDoneSeq;

    /// <summary>
    /// State of the finalizer thread. Claimed with <see cref="Interlocked.CompareExchange{T}"/> by a
    /// start; every change is signalled on <see cref="s_finalizerMonitor"/>.
    /// </summary>
    private static volatile ThreadFinalizerStatus s_threadFinalizerStatus;

    // --- Properties ---

    /// <summary>
    /// Whether the finalizers run on a thread of their own instead of the thread that allocates
    /// or collects. Setting it starts or stops that thread and returns once it is done; turning it
    /// on while the thread is stopping waits for the stop, then starts it again.
    /// </summary>
    /// <remarks>
    /// Reads <see langword="true"/> from the start of a start to the start of a stop. Does
    /// nothing without the scheduler, and turning it off from a finalizer is ignored: the
    /// finalizer thread cannot wait for its own exit.
    /// </remarks>
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
    /// Registers a freshly allocated object for finalization. Does nothing for a type without a
    /// finalizer.
    /// </summary>
    /// <remarks>
    /// Clears the suppressed flag first: the allocator does not zero the runtime header, so the
    /// first object of a recycled block reads the header of the object that died there, which
    /// may have been suppressed.
    /// </remarks>
    /// <param name="obj">The object just allocated.</param>
    internal static void RegisterNewObject(GCObject* obj)
    {
        if (!obj->GetMethodTable()->IsFinalizable)
        {
            return;
        }

        ClearFinalizerSuppressed(obj);
        AddFinalizable(obj);
    }

    /// <summary>
    /// Registers an object for finalization again, <see cref="GC.ReRegisterForFinalize"/>. Does
    /// nothing for a type without a finalizer.
    /// </summary>
    /// <remarks>
    /// As in .NET, a suppressed object has its flag cleared and its registrations stand again;
    /// one that is not suppressed gets one more registration, and its finalizer runs once per
    /// registration. An object a collection dropped while suppressed, kept alive since by
    /// another finalizer, has no registration left and gets one.
    /// </remarks>
    /// <param name="obj">The object to register.</param>
    /// <returns><see langword="false"/> when the registry is full and cannot grow.</returns>
    internal static bool TryRegisterForFinalize(GCObject* obj)
    {
        if (obj == null || !obj->GetMethodTable()->IsFinalizable)
        {
            return true;
        }

        if (IsFinalizerSuppressed(obj))
        {
            ClearFinalizerSuppressed(obj);
            using (InternalCpu.DisableInterruptsScope())
            {
                if (IsRegistered(obj))
                {
                    return true;
                }
            }
        }

        return AddFinalizable(obj);
    }

    /// <summary>
    /// Cancels the finalizer of an object, whether it is still alive or queued already:
    /// sets the suppressed flag in its runtime header, which the collection honours when it
    /// finds the object dead, and a pass when the object's turn comes.
    /// </summary>
    /// <remarks>
    /// Every registration of the object is cancelled, however many times it was registered,
    /// until the next <see cref="TryRegisterForFinalize"/>. The flag stays set on an object a
    /// collection dropped, so the other registrations it had are dropped too, and the allocator
    /// clears it for the next object at that address.
    /// </remarks>
    /// <param name="obj">The object whose finalizer must not run.</param>
    internal static void SuppressFinalize(GCObject* obj)
    {
        if (obj == null || !obj->GetMethodTable()->IsFinalizable)
        {
            return;
        }

        SetFinalizerSuppressed(obj);
    }

    /// <summary>
    /// Returns once the finalizers queued so far have run: asks the finalizer thread for a pass
    /// and waits for it when <see cref="ThreadedFinalizer"/> is on, runs them on the calling
    /// thread otherwise.
    /// </summary>
    /// <remarks>
    /// Returns at once when interrupts are masked, and on the finalizer thread itself.
    /// </remarks>
    internal static void WaitPendingFinalizers()
    {
        // Masked (an IRQ handler, a spin-locked section): the monitor would block a CPU nothing
        // preempts any more, and a finalizer is not to run there either. The finalizer thread
        // would wait for itself.
        if (!InternalCpu.InterruptsEnabled
            || !FinalizationPending()
            || IsOnFinalizerThread())
        {
            return;
        }

        // Running, not Starting: the starter holds the monitor, and may not have created it yet
        ThreadFinalizerStatus status = s_threadFinalizerStatus;
        if (status == ThreadFinalizerStatus.Running)
        {
            Monitor monitor = s_finalizerMonitor!;

            monitor.Acquire();
            // Wraps after 2^64 requests; IsPassDone compares across the wrap
            ulong ticket = unchecked(++s_finalizerRequestSeq);
            s_finalizerRequested = true;
            monitor.SignalAll();

            // Until the pass, or until the thread has exited when a stop cuts in: its last pass
            // holds the running flag to its end, and the fallback would return at once
            monitor.Acquire();
            while (!IsPassDone(s_finalizerDoneSeq, ticket) && !HasExited(s_threadFinalizerStatus))
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
        else if (status == ThreadFinalizerStatus.Stopping)
        {
            // A stop under way: the queue is the thread's until it has exited, for the same
            // reason. A stop means a start ran before, so the monitor exists.
            Monitor monitor = s_finalizerMonitor!;

            monitor.Acquire();
            while (!HasExited(s_threadFinalizerStatus))
            {
                monitor.Wait();
            }
            monitor.Release();

            RunPendingFinalizers();
        }
        else
        {
            RunPendingFinalizers();
        }

        static bool IsPassDone(ulong done, ulong ticket)
        {
            return unchecked((long)(done - ticket)) >= 0;
        }

        // Exited, or Stopped and beyond once the stopper has moved on; Stopping is not enough,
        // the thread's last pass may still be running
        static bool HasExited(ThreadFinalizerStatus status)
        {
            return status is not (ThreadFinalizerStatus.Running or ThreadFinalizerStatus.Stopping);
        }
    }

    /// <summary>
    /// Runs the queued finalizers on the calling thread when <see cref="ThreadedFinalizer"/> is
    /// off; otherwise asks the finalizer thread for a pass, without waiting for it.
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

    /// <summary>
    /// Body of the finalizer thread: runs a pass for each request until the thread is stopped,
    /// then reports <see cref="ThreadFinalizerStatus.Exited"/> to the stopper.
    /// </summary>
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
    /// Starts the finalizer thread, the setter of <see cref="ThreadedFinalizer"/> turning it on.
    /// Does nothing when it is starting or running already.
    /// </summary>
    private static void StartFinalizerThread()
    {
        ThreadFinalizerStatus prev = Interlocked.CompareExchange(ref s_threadFinalizerStatus,
                                                                 ThreadFinalizerStatus.Starting, ThreadFinalizerStatus.Stopped);
        if (prev is ThreadFinalizerStatus.Starting or ThreadFinalizerStatus.Running)
        {
            return;
        }

        if (prev != ThreadFinalizerStatus.Stopped)
        {
            // Stopping or Exited: wait for the stopper to publish Stopped, then claim it. A stop
            // means a start ran before, so the monitor exists. The loop ends on the state word:
            // either this start claims Stopped, or another waiting start claimed it first. The
            // claim under the monitor comes before the first wait: a Stopped published, and
            // signalled, between the claim above and the acquire would otherwise be missed.
            Monitor m = s_finalizerMonitor!;
            m.Acquire();
            prev = Interlocked.CompareExchange(ref s_threadFinalizerStatus,
                                               ThreadFinalizerStatus.Starting, ThreadFinalizerStatus.Stopped);
            while (prev is ThreadFinalizerStatus.Stopping or ThreadFinalizerStatus.Exited)
            {
                m.Wait();
                prev = Interlocked.CompareExchange(ref s_threadFinalizerStatus,
                                                   ThreadFinalizerStatus.Starting, ThreadFinalizerStatus.Stopped);
            }

            m.Release();
            if (prev != ThreadFinalizerStatus.Stopped)
            {
                // Another waiting start claimed it first: Starting or Running
                return;
            }
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
            Serial.WriteString("[GC] WARNING: finalizer thread did not start, finalizers run on the collecting thread\n");
            s_threadFinalizerStatus = ThreadFinalizerStatus.Stopped;
        }

        monitor.SignalAll();
    }

    /// <summary>
    /// Stops the finalizer thread, the setter of <see cref="ThreadedFinalizer"/> turning it off,
    /// and returns once the thread has exited. Its current pass ends first.
    /// </summary>
    private static void StopFinalizerThread()
    {
        Monitor? monitor = s_finalizerMonitor;
        if (monitor is null)
        {
            return;
        }

        // From a finalizer: the thread would wait for its own exit
        if (IsOnFinalizerThread())
        {
            Serial.WriteString("[GC] WARNING: the finalizer thread cannot stop itself, ThreadedFinalizer stays on\n");
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

    /// <summary>
    /// Whether the calling thread is the finalizer thread. False while there is none: without
    /// the scheduler both it and the current thread are null, which is no finalizer thread.
    /// </summary>
    private static bool IsOnFinalizerThread()
    {
        SchedulerThread? finalizerThread = s_finalizerThread;
        return finalizerThread is not null
            && ReferenceEquals(SchedulerManager.CurrentCpuState?.CurrentThread, finalizerThread);
    }

    /// <summary>
    /// Whether a finalizer is queued or a pass is running.
    /// </summary>
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
    /// <para>
    /// One pass at a time relies on a single CPU: <see cref="s_runningFinalizers"/> is claimed
    /// with interrupts masked, which excludes the other threads only while they share that CPU.
    /// </para>
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

        while (target is not null)
        {
            try
            {
                ((delegate*<object, void>)finalizer)(target);
            }
            catch (Exception)
            {
                // .NET fails fast on an exception a finalizer lets escape; the kernel logs it and
                // goes on with the next finalizer, so one faulty type does not stop finalization
                Serial.WriteString("[GC] WARNING: unhandled exception in a finalizer\n");
            }

            target = DequeueFinalizable(out finalizer);
        }
    }

    /// <summary>
    /// Takes the next object to finalize off the queue, skipping those suppressed since they
    /// were queued. An empty queue ends the pass: the running flag is released in the same
    /// step, so an object queued right after is not left behind by a pass that had already seen
    /// the queue empty.
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
                if (IsFinalizerSuppressed(entry))
                {
                    // Suppressed from another finalizer, or through a resurrection-tracking
                    // weak reference. Cleared, as .NET does: registered again, it is queued again.
                    ClearFinalizerSuppressed(entry);
                    continue;
                }

                finalizer = entry->GetMethodTable()->FinalizerCode;
                nint obj = (nint)entry;
                return Unsafe.As<nint, object>(ref obj);
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
    /// what they reference: they stay alive until their finalizer has run. A dead object whose
    /// finalizer was suppressed is dropped instead, and swept. Runs once the roots are marked
    /// and the short weak handles are cleared.
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
            GCObject* obj = s_finalizable.Items[i];
            if (!obj->IsMarked && !IsFinalizerSuppressed(obj))
            {
                dead++;
            }
        }

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
                else if (!IsFinalizerSuppressed(obj))
                {
                    s_finalizerQueue.Items[s_finalizerQueue.Count++] = obj;
                }
            }

            s_finalizable.Count = kept;
            if (dead != 0)
            {
                MarkFinalizerQueue();
            }
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

        s_lastFinalizationPendingCount = s_finalizerQueue.Count - s_finalizerQueueHead;
    }

    /// <summary>
    /// Adds an object to the registry, with a warning when the registry cannot grow.
    /// </summary>
    /// <param name="obj">The object to register.</param>
    /// <returns><see langword="false"/> when the registry is full and cannot grow.</returns>
    private static bool AddFinalizable(GCObject* obj)
    {
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
    /// Whether an object has a registration left, in the registry or in the queue. Called with
    /// interrupts masked; linear, on the re-registration of a suppressed object only.
    /// </summary>
    /// <param name="obj">The object to look for.</param>
    private static bool IsRegistered(GCObject* obj)
    {
        for (int i = 0; i < s_finalizable.Count; i++)
        {
            if (s_finalizable.Items[i] == obj)
            {
                return true;
            }
        }

        for (int i = s_finalizerQueueHead; i < s_finalizerQueue.Count; i++)
        {
            if (s_finalizerQueue.Items[i] == obj)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The runtime object header of an object: the 4 bytes before it, see
    /// <see cref="FinalizerSuppressedBit"/>.
    /// </summary>
    private static int* RuntimeHeaderOf(GCObject* obj)
    {
        return (int*)obj - 1;
    }

    /// <summary>
    /// Whether <see cref="SuppressFinalize"/> was called on the object since its last
    /// registration.
    /// </summary>
    private static bool IsFinalizerSuppressed(GCObject* obj)
    {
        return (*RuntimeHeaderOf(obj) & FinalizerSuppressedBit) != 0;
    }

    /// <summary>
    /// Sets the suppressed flag. Atomic against CoreLib's compare-exchange on the same word (a
    /// thin lock taken or released, a hash code assigned).
    /// </summary>
    private static void SetFinalizerSuppressed(GCObject* obj)
    {
        Interlocked.Or(ref *RuntimeHeaderOf(obj), FinalizerSuppressedBit);
    }

    /// <summary>
    /// Clears the suppressed flag, see <see cref="SetFinalizerSuppressed"/>. A plain read first:
    /// the flag is clear on most objects, and a read costs nothing.
    /// </summary>
    private static void ClearFinalizerSuppressed(GCObject* obj)
    {
        if (IsFinalizerSuppressed(obj))
        {
            Interlocked.And(ref *RuntimeHeaderOf(obj), ~FinalizerSuppressedBit);
        }
    }
}
