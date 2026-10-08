// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

#pragma warning disable CS8500

/// <summary>
/// Mark phase: root scanning, reference enumeration, and mark stack management.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    /// <summary>
    /// Executes the mark phase: scans roots (stack, GC handles) and marks all reachable objects.
    /// Static roots need no separate pass: <c>ManagedModule.InitializeStatics</c> holds every
    /// module's GC-statics base objects in a spine array behind a strong GC handle, so the
    /// handle scan reaches all objects referenced by static fields transitively (proven by the
    /// GC_StaticOnlyReachability kernel test).
    /// </summary>
    private static void MarkPhase()
    {
        s_markStackCount = 0;
        s_markStackFull = false;
        s_markOverflowMin = null;
        s_markOverflowMax = null;
        ScanStackRoots();
        ScanGCHandles();
    }

    /// <summary>
    /// Scans all GC handle entries: marks objects referenced by strong handles (Normal and Pinned),
    /// then runs a convergence loop for dependent handles (if primary is marked, mark secondary).
    /// Weak handles are skipped so their targets can be collected if otherwise unreachable.
    /// </summary>
    private static void ScanGCHandles()
    {
        ScanGCHandles(GCHandleType.Normal);
        ScanGCHandles(GCHandleType.Pinned);

        // Pass 2: Dependent handle convergence loop
        // If primary is live, mark secondary. Repeat until no new marks (handles transitive chains).
        bool markedNew = true;
        while (markedNew)
        {
            markedNew = false;
            var storeEnum = s_gCHandleManager.DependentHandleStore.GetEnumerator();
            while (storeEnum.MoveNext())
            {
                // No secondary means nothing to keep alive: ConditionalWeakTable stores a null
                // value as a dependent handle without one (SharedArrayPool registers its
                // thread-local buckets that way), so the mark bit must not be read through it;
                // nor through a null primary, which a collection leaves when the primary dies.
                GCObject* primary = storeEnum.Current->Object;
                GCObject* secondary = (GCObject*)storeEnum.Current->ExtraInfo;
                if (secondary != null && primary != null && IsLive(primary) && !secondary->IsMarked)
                {
                    TryMarkRoot((nint)secondary);

                    // Another pass only for a secondary marked now: one TryMarkRoot leaves unmarked
                    // (a frozen object, outside the GC heap) stays so, and counting it looped here
                    // for ever with interrupts masked, the whole machine stopped.
                    markedNew |= secondary->IsMarked;
                }
            }
        }

        void ScanGCHandles(GCHandleType type)
        {
            var storeEnum = s_gCHandleManager.GCHandleControllers[(int)type].GetEnumerator();
            while (storeEnum.MoveNext())
            {
                TryMarkRoot((nint)storeEnum.Current->Object);
            }
        }
    }

    /// <summary>
    /// Scans stack roots. The GC-triggering (current) thread is scanned precisely from NativeAOT
    /// GCInfo — it is parked at a call-site safepoint, so the scan is sound — while every other
    /// registered thread (preempted at an arbitrary IP, where its GCInfo lookup may be meaningless)
    /// still gets a conservative word-by-word scan of its saved registers and stack until
    /// return-address hijacking lands. See issue #385.
    /// <para>
    /// <see cref="GarbageCollector.Collect"/> runs this inside <c>InternalCpu.DisableInterruptsScope()</c>,
    /// so the other registered threads are quiescent for the duration.
    /// </para>
    /// </summary>
    private static void ScanStackRoots()
    {
        if (CosmosFeatures.SchedulerEnabled && SchedulerManager.IsEnabled)
        {
            SchedulerThread? current = SchedulerManager.CurrentCpuState?.CurrentThread;

            nuint stackEnd;
            if (current is not null && current.StackBase != 0 && current.StackSize != 0)
            {
                stackEnd = current.StackBase + current.StackSize;
            }
            else
            {
                // Boot/idle thread on the bootloader stack — no StackBase/StackSize.
                stackEnd = GetCurrentStackEndForBootStack();
            }

            PreciseScanCurrentThread(stackEnd);

            var threads = SchedulerManager.Threads;
            if (threads is not null)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    var thread = threads[i];
                    if (thread is not null && !ReferenceEquals(thread, current) && thread.State != SchedulerThreadState.Dead)
                    {
                        ScanThreadStack(thread);
                    }
                }
            }
        }
        else
        {
            // No scheduler — only one stack, and it is the GC-triggering thread's.
            PreciseScanCurrentThread(GetCurrentStackEndForBootStack());
        }
    }

    /// <summary>
    /// Stack-end bound for the current thread when its <see cref="SchedulerThread"/> has no
    /// allocated <c>StackBase</c>/<c>StackSize</c> — the boot/idle thread runs on the bootloader's
    /// stack, whose top kmain captured before any managed code ran.
    /// </summary>
    private static nuint GetCurrentStackEndForBootStack()
    {
        return Runtime.BootStack.Top;
    }

    /// <summary>
    /// Scans a thread's saved register state and stack for potential object references.
    /// </summary>
    /// <param name="thread">The thread whose stack and registers to scan.</param>
    private static void ScanThreadStack(SchedulerThread thread)
    {
        if (thread is null)
        {
            return;
        }

        // ScanStackRoots never passes the current thread, so a thread still marked Running here is
        // another CPU's: the idle thread of an application processor, which Cosmos does not start
        // yet. It never ran, so it has no saved context and no stack. Scanning it from this CPU's
        // stack pointer up to BootStack.Top read, on a multi-core machine, whatever memory lies
        // between the collecting thread's stack and the boot stack.
        if (thread.State == SchedulerThreadState.Running)
        {
            return;
        }

        Scheduler.ThreadContext* ctx = thread.GetContext();
        if (ctx != null)
        {
#if ARCH_ARM64
            // Scan all general-purpose registers X0-X30
            TryMarkConservativeRoot((nint)ctx->X0);
            TryMarkConservativeRoot((nint)ctx->X1);
            TryMarkConservativeRoot((nint)ctx->X2);
            TryMarkConservativeRoot((nint)ctx->X3);
            TryMarkConservativeRoot((nint)ctx->X4);
            TryMarkConservativeRoot((nint)ctx->X5);
            TryMarkConservativeRoot((nint)ctx->X6);
            TryMarkConservativeRoot((nint)ctx->X7);
            TryMarkConservativeRoot((nint)ctx->X8);
            TryMarkConservativeRoot((nint)ctx->X9);
            TryMarkConservativeRoot((nint)ctx->X10);
            TryMarkConservativeRoot((nint)ctx->X11);
            TryMarkConservativeRoot((nint)ctx->X12);
            TryMarkConservativeRoot((nint)ctx->X13);
            TryMarkConservativeRoot((nint)ctx->X14);
            TryMarkConservativeRoot((nint)ctx->X15);
            TryMarkConservativeRoot((nint)ctx->X16);
            TryMarkConservativeRoot((nint)ctx->X17);
            TryMarkConservativeRoot((nint)ctx->X18);
            TryMarkConservativeRoot((nint)ctx->X19);
            TryMarkConservativeRoot((nint)ctx->X20);
            TryMarkConservativeRoot((nint)ctx->X21);
            TryMarkConservativeRoot((nint)ctx->X22);
            TryMarkConservativeRoot((nint)ctx->X23);
            TryMarkConservativeRoot((nint)ctx->X24);
            TryMarkConservativeRoot((nint)ctx->X25);
            TryMarkConservativeRoot((nint)ctx->X26);
            TryMarkConservativeRoot((nint)ctx->X27);
            TryMarkConservativeRoot((nint)ctx->X28);
            TryMarkConservativeRoot((nint)ctx->X29);  // FP (Frame Pointer)
            TryMarkConservativeRoot((nint)ctx->X30);  // LR (Link Register)
            TryMarkConservativeRoot((nint)ctx->Sp);   // Stack Pointer
            TryMarkConservativeRoot((nint)ctx->Elr);  // Exception Link Register (return address)
#else
            // x64: Scan all general-purpose registers
            TryMarkConservativeRoot((nint)ctx->Rax);
            TryMarkConservativeRoot((nint)ctx->Rbx);
            TryMarkConservativeRoot((nint)ctx->Rcx);
            TryMarkConservativeRoot((nint)ctx->Rdx);
            TryMarkConservativeRoot((nint)ctx->Rsi);
            TryMarkConservativeRoot((nint)ctx->Rdi);
            TryMarkConservativeRoot((nint)ctx->Rbp);
            TryMarkConservativeRoot((nint)ctx->R8);
            TryMarkConservativeRoot((nint)ctx->R9);
            TryMarkConservativeRoot((nint)ctx->R10);
            TryMarkConservativeRoot((nint)ctx->R11);
            TryMarkConservativeRoot((nint)ctx->R12);
            TryMarkConservativeRoot((nint)ctx->R13);
            TryMarkConservativeRoot((nint)ctx->R14);
            TryMarkConservativeRoot((nint)ctx->R15);
#endif
        }

        // The saved context, and the suspended thread's frames above it, start at StackPointer.
        nuint stackStart = thread.StackPointer;
        nuint stackEnd;
        if (thread.StackBase != 0 && thread.StackSize != 0)
        {
            stackEnd = thread.StackBase + thread.StackSize;
        }
        else if (stackStart != 0 && thread.CpuId == 0 && (thread.Flags & SchedulerThreadFlags.IdleThread) != 0)
        {
            // The boot/idle thread (the kernel's main thread) has no StackBase/StackSize: it
            // runs on the bootloader stack, where the IRQ stub that switched it out saved its
            // context, at StackPointer. Skipping it lost every main-thread stack root whenever
            // another thread collected.
            stackEnd = GetCurrentStackEndForBootStack();
        }
        else
        {
            return;
        }

        if (stackStart < stackEnd)
        {
            ScanMemoryRange((nint*)stackStart, (nint*)stackEnd);
        }
    }

    /// <summary>
    /// Scans a contiguous memory range for potential object references (conservative scanning).
    /// </summary>
    /// <param name="start">Pointer to the first word to scan.</param>
    /// <param name="end">Pointer past the last word to scan.</param>
    private static void ScanMemoryRange(nint* start, nint* end)
    {
        for (nint* ptr = start; ptr < end; ptr++)
        {
            TryMarkConservativeRoot(*ptr);
        }
    }

    /// <summary>
    /// Marks the object a conservatively scanned word (a stack slot, a saved register) refers to.
    /// The word may be an object reference, an interior pointer (a byref, a span, the <c>this</c>
    /// of a struct method called on a field) or any value that happens to fall in the heap, so it
    /// is first resolved to the object whose extent contains it by <see cref="GetParentObject"/>.
    /// Handing the raw value to <see cref="TryMarkRoot"/> read whatever word it pointed at as a
    /// MethodTable: a field holding a kernel address (the GCHandle in
    /// <c>ThreadWaitInfo._waitMonitor</c>, reached through the <c>LowLevelMonitor</c> byref a
    /// waiting thread keeps in a callee-saved register) passed the MethodTable checks and had the
    /// mark bit ORed into it for good, since the sweep only unmarks object starts; and the object
    /// that contains an interior pointer was not kept alive.
    /// </summary>
    /// <param name="value">The word found on the stack or in a saved register.</param>
    private static void TryMarkConservativeRoot(nint value)
    {
        if (!IsInGCHeap(value))
        {
            return;
        }

        GCObject* obj = GetParentObject((byte*)value);
        if (obj != null)
        {
            TryMarkRoot((nint)obj);
        }
    }

    /// <summary>
    /// Marks an object and everything reachable from it. <paramref name="value"/> must be an
    /// object start (a handle target, a precise GCInfo slot, a GCDesc reference); a word found by
    /// conservative scanning goes through <see cref="TryMarkConservativeRoot"/> first. Uses an
    /// iterative mark stack to avoid deep recursion; what a full stack cannot take is found again
    /// by <see cref="ProcessMarkOverflow"/> before this returns.
    /// </summary>
    /// <param name="value">Object pointer to mark.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static void TryMarkRoot(nint value)
    {
        // Conservative scanning: only consider values that point into the GC heap.
        // Stack slots may contain arbitrary integers, return addresses, etc.
        if (!IsInGCHeap(value))
        {
            return;
        }

        MarkAndPush((GCObject*)value);
        DrainMarkStack();

        while (s_markOverflowMin != null)
        {
            ProcessMarkOverflow();
        }
    }

    /// <summary>
    /// Marks an object the mark phase reached and, when it has references, pushes it for
    /// <see cref="DrainMarkStack"/> to follow them. Validates that the pointer looks like a valid
    /// GC object (MethodTable outside heap) first. An object is marked when it is pushed, as the
    /// .NET collector does, so a reference to an object reached already is never pushed again, and
    /// a rescan after an overflow only pushes what is still unmarked.
    /// </summary>
    /// <param name="obj">The object reached.</param>
    private static void MarkAndPush(GCObject* obj)
    {
        // Validate MethodTable - must point outside heap (to kernel code)
        nuint mtPtr = (nuint)obj->MethodTable & ~(nuint)1;
        if (mtPtr == 0 || IsInGCHeap((nint)mtPtr))
        {
            return;
        }

        // MethodTable must be in kernel address space (higher-half).
        // Reject pointers in userspace range — they're garbage from conservative scanning.
        if (mtPtr < AddressSpace.KernelSpaceStart)
        {
            return;
        }

        if (obj->IsMarked)
        {
            return;
        }

        obj->Mark();

        if (((MethodTable*)mtPtr)->ContainsGCPointers && !PushMarkStack((nint)obj))
        {
            NoteMarkOverflow(obj);
        }
    }

    /// <summary>
    /// Follows the references of the objects on the mark stack until it is empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static void DrainMarkStack()
    {
        while (s_markStackCount > 0)
        {
            var obj = (GCObject*)PopMarkStack();
            EnumerateReferences(obj, obj->GetMethodTable());
        }
    }

    /// <summary>
    /// Records a marked object whose references a full mark stack left unfollowed.
    /// </summary>
    /// <param name="obj">The object that could not be pushed.</param>
    private static void NoteMarkOverflow(GCObject* obj)
    {
        byte* p = (byte*)obj;
        if (s_markOverflowMin == null || p < s_markOverflowMin)
        {
            s_markOverflowMin = p;
        }

        if (p > s_markOverflowMax)
        {
            s_markOverflowMax = p;
        }
    }

    /// <summary>
    /// Follows the references of the marked objects a full mark stack could not take, as the .NET
    /// collector recovers from a mark stack overflow. Those objects lie between
    /// <see cref="s_markOverflowMin"/> and <see cref="s_markOverflowMax"/>: the heap is walked over
    /// that range, and every marked object with references there has them followed again, the
    /// stack drained after each. An object scanned already finds its references marked and pushes
    /// nothing; the objects the stack overflows on meanwhile make up the range of the next round.
    /// </summary>
    private static void ProcessMarkOverflow()
    {
        byte* min = s_markOverflowMin;
        byte* max = s_markOverflowMax;
        s_markOverflowMin = null;
        s_markOverflowMax = null;

        RescanMarkedObjects(s_segmentManager.Segments, min, max);
        RescanMarkedObjects(s_pinnedSegmentManager.Segments, min, max);
    }

    /// <summary>
    /// Follows again the references of the marked objects of a segment list that lie between
    /// <paramref name="min"/> and <paramref name="max"/>.
    /// </summary>
    /// <param name="segment">The first segment of the list.</param>
    /// <param name="min">The lowest object address to rescan.</param>
    /// <param name="max">The highest object address to rescan.</param>
    private static void RescanMarkedObjects(GCSegment* segment, byte* min, byte* max)
    {
        for (; segment != null; segment = segment->Next)
        {
            if (segment->Bump <= min || segment->Start > max)
            {
                continue;
            }

            byte* ptr = segment->Start;
            while (ptr < segment->Bump && ptr <= max)
            {
                uint size = GetHeapEntrySize(segment, ptr, out bool isObject);
                if (size == 0)
                {
                    break;
                }

                var obj = (GCObject*)ptr;
                if (isObject && ptr >= min && obj->IsMarked)
                {
                    MethodTable* mt = obj->GetMethodTable();
                    if (mt->ContainsGCPointers)
                    {
                        EnumerateReferences(obj, mt);
                        DrainMarkStack();
                    }
                }

                ptr += size;
            }
        }
    }

    /// <summary>
    /// Enumerates object references described by the GCDesc and marks them, pushing those with
    /// references of their own onto the mark stack.
    /// Handles both fixed-layout objects (positive series count) and arrays of structs (negative series count).
    /// </summary>
    /// <param name="obj">The object whose references to enumerate.</param>
    /// <param name="mt">The object's MethodTable (must have <c>ContainsGCPointers</c> set).</param>
    private static void EnumerateReferences(GCObject* obj, MethodTable* mt)
    {
        nint numSeries = ((nint*)mt)[-1];
        if (numSeries == 0)
        {
            return;
        }

        var cur = (GCDescSeries*)((nint*)mt - 1) - 1;

        if (numSeries > 0)
        {
            uint objectSize = obj->ComputeSize();
            GCDescSeries* last = cur - numSeries + 1;

            do
            {
                nint size = cur->SeriesSize + (nint)objectSize;
                nint offset = cur->StartOffset;
                var ptr = (nint*)((nint)obj + offset);

                for (nint i = 0; i < size / IntPtr.Size; i++)
                {
                    nint refValue = ptr[i];
                    if (refValue != 0 && IsInGCHeap(refValue))
                    {
                        MarkAndPush((GCObject*)refValue);
                    }
                }

                cur--;
            } while (cur >= last);
        }
        else
        {
            nint offset = ((nint*)mt)[-2];
            var valSeries = (ValSerieItem*)((nint*)mt - 2) - 1;

            // Start at the offset
            var ptr = (nint*)((nint)obj + offset);

            // Retrieve the length of the array
            int length = obj->Length;

            // Repeat the loop for each element in the array
            for (int item = 0; item < length; item++)
            {
                for (int i = 0; i > numSeries; i--)
                {
                    // i is negative, so this is going backwards
                    ValSerieItem* valSerieItem = valSeries + i;

                    // Read valSerieItem->Nptrs pointers
                    for (int j = 0; j < valSerieItem->Nptrs; j++)
                    {
                        nint refValue = *ptr;
                        if (refValue != 0 && IsInGCHeap(refValue))
                        {
                            MarkAndPush((GCObject*)refValue);
                        }

                        ptr++;
                    }

                    // Skip valSerieItem->Skip bytes
                    ptr = (nint*)((nint)ptr + valSerieItem->Skip);
                }
            }
        }
    }

    /// <summary>
    /// Pushes an object onto the mark stack. Expands the stack if full.
    /// </summary>
    /// <param name="ptr">The object to push.</param>
    /// <returns>
    /// <c>false</c> when the stack is full and cannot grow: the caller records the overflow for
    /// <see cref="ProcessMarkOverflow"/>.
    /// </returns>
    private static bool PushMarkStack(nint ptr)
    {
        if (s_markStackCount >= s_markStackCapacity)
        {
            if (s_markStackFull)
            {
                return false;
            }

            // Expand mark stack
            ulong newPageCount = (s_markStackPageCount + 1) * 2;
            nint* newStack = (nint*)PageAllocator.AllocPages(PageType.Unmanaged, newPageCount, true);
            if (newStack == null)
            {
                Serial.WriteString("[GC] WARNING: Mark stack overflow\n");
                s_markStackFull = true;
                return false;
            }

            for (int i = 0; i < s_markStackCount; i++)
            {
                newStack[i] = s_markStack[i];
            }

            PageAllocator.Free(s_markStack);
            s_markStack = newStack;
            s_markStackCapacity = (int)(newPageCount * PageAllocator.PageSize / (ulong)sizeof(nint));
            s_markStackPageCount = newPageCount;
        }

        s_markStack[s_markStackCount++] = ptr;
        return true;
    }

    /// <summary>
    /// Pops the top entry from the mark stack.
    /// </summary>
    /// <returns>The popped pointer value, or <c>0</c> if the stack is empty.</returns>
    private static nint PopMarkStack()
    {
        return s_markStackCount > 0 ? s_markStack[--s_markStackCount] : 0;
    }
}
