// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge;
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
        // If primary is marked, mark secondary. Repeat until no new marks (handles transitive chains).
        bool markedNew = true;
        while (markedNew)
        {
            markedNew = false;
            var storeEnum = s_gCHandleManager.DependentHandleStore.GetEnumerator();
            while (storeEnum.MoveNext())
            {
                // No secondary means nothing to keep alive: ConditionalWeakTable stores a null
                // value as a dependent handle without one (SharedArrayPool registers its
                // thread-local buckets that way), so the mark bit must not be read through it.
                GCObject* secondary = (GCObject*)storeEnum.Current->ExtraInfo;
                if (secondary != null && storeEnum.Current->Object->IsMarked && !secondary->IsMarked)
                {
                    TryMarkRoot((nint)secondary);
                    markedNew = true;
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
            if (current != null && current.StackBase != 0 && current.StackSize != 0)
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
            if (threads != null)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    var thread = threads[i];
                    if (thread is not null && !object.ReferenceEquals(thread, current) && thread.State != SchedulerThreadState.Dead)
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
        if (thread == null)
        {
            return;
        }

        if (thread.State != SchedulerThreadState.Running)
        {
            Scheduler.ThreadContext* ctx = thread.GetContext();
            if (ctx != null)
            {
#if ARCH_ARM64
                // Scan all general-purpose registers X0-X30
                TryMarkRoot((nint)ctx->X0);
                TryMarkRoot((nint)ctx->X1);
                TryMarkRoot((nint)ctx->X2);
                TryMarkRoot((nint)ctx->X3);
                TryMarkRoot((nint)ctx->X4);
                TryMarkRoot((nint)ctx->X5);
                TryMarkRoot((nint)ctx->X6);
                TryMarkRoot((nint)ctx->X7);
                TryMarkRoot((nint)ctx->X8);
                TryMarkRoot((nint)ctx->X9);
                TryMarkRoot((nint)ctx->X10);
                TryMarkRoot((nint)ctx->X11);
                TryMarkRoot((nint)ctx->X12);
                TryMarkRoot((nint)ctx->X13);
                TryMarkRoot((nint)ctx->X14);
                TryMarkRoot((nint)ctx->X15);
                TryMarkRoot((nint)ctx->X16);
                TryMarkRoot((nint)ctx->X17);
                TryMarkRoot((nint)ctx->X18);
                TryMarkRoot((nint)ctx->X19);
                TryMarkRoot((nint)ctx->X20);
                TryMarkRoot((nint)ctx->X21);
                TryMarkRoot((nint)ctx->X22);
                TryMarkRoot((nint)ctx->X23);
                TryMarkRoot((nint)ctx->X24);
                TryMarkRoot((nint)ctx->X25);
                TryMarkRoot((nint)ctx->X26);
                TryMarkRoot((nint)ctx->X27);
                TryMarkRoot((nint)ctx->X28);
                TryMarkRoot((nint)ctx->X29);  // FP (Frame Pointer)
                TryMarkRoot((nint)ctx->X30);  // LR (Link Register)
                TryMarkRoot((nint)ctx->Sp);   // Stack Pointer
                TryMarkRoot((nint)ctx->Elr);  // Exception Link Register (return address)
#else
                // x64: Scan all general-purpose registers
                TryMarkRoot((nint)ctx->Rax);
                TryMarkRoot((nint)ctx->Rbx);
                TryMarkRoot((nint)ctx->Rcx);
                TryMarkRoot((nint)ctx->Rdx);
                TryMarkRoot((nint)ctx->Rsi);
                TryMarkRoot((nint)ctx->Rdi);
                TryMarkRoot((nint)ctx->Rbp);
                TryMarkRoot((nint)ctx->R8);
                TryMarkRoot((nint)ctx->R9);
                TryMarkRoot((nint)ctx->R10);
                TryMarkRoot((nint)ctx->R11);
                TryMarkRoot((nint)ctx->R12);
                TryMarkRoot((nint)ctx->R13);
                TryMarkRoot((nint)ctx->R14);
                TryMarkRoot((nint)ctx->R15);
#endif
            }
        }

        // Determine stack range to scan
        nuint stackStart;
        nuint stackEnd;

        if (thread.State == SchedulerThreadState.Running)
        {
            // For the currently running thread, thread.StackPointer is stale (saved
            // during the last context switch). Use the actual RSP instead.
            stackStart = ContextSwitchNative.GetSp();

            if (thread.StackBase != 0 && thread.StackSize != 0)
            {
                // Regular scheduled thread with allocated stack
                stackEnd = thread.StackBase + thread.StackSize;
            }
            else
            {
                // Boot/idle thread uses the bootloader's stack — no StackBase/StackSize.
                stackEnd = GetCurrentStackEndForBootStack();
            }
        }
        else
        {
            // Suspended thread — use saved StackPointer
            if (thread.StackBase == 0 || thread.StackSize == 0)
            {
                return;
            }

            stackStart = thread.StackPointer;
            stackEnd = thread.StackBase + thread.StackSize;
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
            TryMarkRoot(*ptr);
        }
    }

    /// <summary>
    /// Attempts to mark a potential object reference. Validates that the pointer looks like a
    /// valid GC object (MethodTable outside heap) before marking and enumerating its references.
    /// Uses an iterative mark stack to avoid deep recursion.
    /// </summary>
    /// <param name="value">Potential object pointer to investigate.</param>
    [MethodImpl(MethodImplOptions.NoOptimization)]
    private static void TryMarkRoot(nint value)
    {
        // Conservative scanning: only consider values that point into the GC heap.
        // Stack slots may contain arbitrary integers, return addresses, etc.
        if (!IsInGCHeap(value))
        {
            return;
        }

        PushMarkStack(value);

        while (s_markStackCount > 0)
        {
            nint ptr = PopMarkStack();
            var obj = (GCObject*)ptr;

            // Validate MethodTable - must point outside heap (to kernel code)
            nuint mtPtr = (nuint)obj->MethodTable & ~(nuint)1;
            if (mtPtr == 0 || IsInGCHeap((nint)mtPtr))
            {
                continue;
            }

            // MethodTable must be in kernel address space (higher-half).
            // Reject pointers in userspace range — they're garbage from conservative scanning.
            if (mtPtr < AddressSpace.KernelSpaceStart)
            {
                continue;
            }

            if (obj->IsMarked)
            {
                continue;
            }

            obj->Mark();

            MethodTable* mt = obj->GetMethodTable();
            if (mt->ContainsGCPointers)
            {
                EnumerateReferences(obj, mt);
            }
        }
    }

    /// <summary>
    /// Enumerates object references described by the GCDesc and pushes them onto the mark stack.
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
                        PushMarkStack(refValue);
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
                            PushMarkStack(refValue);
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
    /// Pushes a potential object pointer onto the mark stack. Expands the stack if full.
    /// </summary>
    /// <param name="ptr">The pointer to push.</param>
    private static void PushMarkStack(nint ptr)
    {
        if (s_markStackCount >= s_markStackCapacity)
        {
            // Expand mark stack
            ulong newPageCount = (s_markStackPageCount + 1) * 2;
            nint* newStack = (nint*)PageAllocator.AllocPages(PageType.Unmanaged, newPageCount, true);
            if (newStack == null)
            {
                Serial.WriteString("[GC] WARNING: Mark stack overflow\n");
                return;
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
