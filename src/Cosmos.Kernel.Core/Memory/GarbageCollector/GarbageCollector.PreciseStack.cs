// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.Core.Runtime.GcInfo;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

#pragma warning disable CS8500

/// <summary>
/// Precise GC stack scanning for the GC-triggering thread, driven by NativeAOT GCInfo (issue #346).
/// Walks the thread's managed frames one at a time with the CFI unwinder from
/// <see cref="ExceptionHelper"/>, decoding each method's GCInfo at the matching instruction pointer
/// — including exception-funclet frames (catch / filter / finally bodies), which share their main
/// method's slot table — and reporting only the slots it names. A CFI-described asm trampoline (the
/// funclet-invoke stubs) carries no GCInfo of its own; the walk steps through it reporting nothing —
/// its frame holds no managed references the managed frames on either side don't already cover.
/// Falls back to a conservative scan of the remaining stack only when the walk leaves everything it
/// can describe (asm entry stubs, native imports, IRQ entry — no CFI at all) or a frame's slot table
/// is too large to decode.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    private const int MaxPreciseFrames = 256;

    private enum FrameResult
    {
        Continue,
        Stop,
    }

    /// <summary>
    /// Precisely scans the GC-triggering (current) thread. Captures this frame's register context
    /// via <see cref="ContextSwitchNative.CaptureRegDisplay"/> and walks up from there.
    /// <paramref name="threadStackEnd"/> bounds the conservative fallback tail (top-of-stack address).
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void PreciseScanCurrentThread(nuint threadStackEnd)
    {
        REGDISPLAY rd = default;
        nuint ip = ContextSwitchNative.CaptureRegDisplay(&rd);
        PreciseScanFrameChain(&rd, ip, threadStackEnd);
    }

    /// <summary>
    /// Walks managed frames starting from <paramref name="rd0"/> / <paramref name="startIp"/>,
    /// precisely reporting GC slots per frame and CFI-unwinding to the caller until the walk leaves
    /// managed code or hits a bail-out condition.
    /// </summary>
    private static void PreciseScanFrameChain(REGDISPLAY* rd0, nuint startIp, nuint threadStackEnd)
    {
        nuint ip = startIp;

        // Frame 0 — the captured frame; its REGDISPLAY came straight from the capture stub.
        if (RunFrame(rd0, ip, threadStackEnd) == FrameResult.Stop)
        {
            return;
        }

        UnwindState st = default;
        ExceptionHelper.SeedUnwindStateFromRegDisplay(ref st, rd0, ip);

        nuint prevSp = rd0->SP;
        for (int i = 0; i < MaxPreciseFrames; i++)
        {
            if (!ExceptionHelper.UnwindOneFrameWithCFI(ref st, ip))
            {
                // No CFI for this IP — treat the rest of the stack conservatively and stop.
                if (prevSp != 0 && prevSp < threadStackEnd)
                {
                    ScanMemoryRange((nint*)prevSp, (nint*)threadStackEnd);
                }
                return;
            }

            nuint sp = st.StackPointer;
            ip = st.ReturnAddress;

            // Stack grows down: each older frame has a strictly higher SP. Anything else is a sign
            // the unwind produced garbage — bail rather than chase it.
            if (ip == 0 || sp == 0 || sp <= prevSp || sp >= threadStackEnd)
            {
                return;
            }
            prevSp = sp;

            REGDISPLAY rd = default;
            ExceptionHelper.ProjectRegDisplay(ref st, &rd);
            if (RunFrame(&rd, ip, threadStackEnd) == FrameResult.Stop)
            {
                return;
            }
        }
    }

    /// <summary>
    /// Reports the GC roots live in one frame. Returns <see cref="FrameResult.Stop"/> (after a
    /// conservative scan of the remaining stack) when the frame has no usable GCInfo.
    /// </summary>
    private static FrameResult RunFrame(REGDISPLAY* rd, nuint ip, nuint threadStackEnd)
    {
        nuint sp = rd->SP;
        if (sp == 0 || sp >= threadStackEnd)
        {
            return FrameResult.Stop;
        }

        if (!MethodGcInfoLookup.TryGetMethodGcInfo(ip, out MethodGcInfoLookup.MethodGcInfo mi) || mi.GcInfo == null)
        {
            // No GCInfo for this IP. A hand-written asm trampoline (RhpCallCatchFunclet /
            // RhpCallFilterFunclet / RhpThrowEx) still carries .cfi directives — it just has no
            // .dotnet_eh_table LSDA. Its own frame holds no managed references the managed frames on
            // either side don't already report via their register save locations (the in-flight
            // exception object is reported by the funclet's / RhThrowEx's `ex` slot deeper on the
            // stack), so step through it and keep the precise walk going. Only when there is no CFI
            // at all (IRQ entry, the bootloader) do we conservatively scan the rest and stop.
            if (MethodGcInfoLookup.TryGetMethodCFI(ip, out _))
            {
                return FrameResult.Continue;
            }
            ScanMemoryRange((nint*)sp, (nint*)threadStackEnd);
            return FrameResult.Stop;
        }

        // A funclet (catch / filter / finally body) carries no GCInfo of its own — it shares the
        // main method's slot table at a synthetic code offset past the main body (MethodGcInfoLookup
        // already resolved mi.CodeOffset against the main method start). It runs on the establisher
        // (main) frame's frame register — RhpCallCatchFunclet restores it and the funclet sets up no
        // frame of its own — so the main method's frame-relative slots resolve against this frame's
        // REGDISPLAY. A filter runs mid-throw, so the main method's *untracked* slots may be stale
        // and must not be reported (NoReportUntracked); a catch / finally funclet re-reports them —
        // the parent frame's own scan reports them again too, but mark is idempotent. Mirrors
        // UnixNativeCodeManager::EnumGcRefs; this is the scan path that targets issue #227.
        CodeManagerFlags flags = (mi.IsFunclet && mi.IsFilter) ? CodeManagerFlags.NoReportUntracked : CodeManagerFlags.None;

        GcInfoDecoder decoder = new(mi.GcInfo, GcInfoEncoding.GCINFO_VERSION, GcInfoDecoderFlags.DECODE_GC_LIFETIMES, mi.CodeOffset);
        bool fit = decoder.EnumerateLiveSlots(rd, reportScratchSlots: false, flags, &PreciseRootTrampoline, null);
        if (!fit)
        {
            // Slot table overflowed the decoder's fixed buffer — fall back conservatively for the rest.
            ScanMemoryRange((nint*)sp, (nint*)threadStackEnd);
            return FrameResult.Stop;
        }

        return FrameResult.Continue;
    }

    /// <summary>
    /// <see cref="GcInfoDecoder.EnumerateLiveSlots"/> callback: marks each reported root via
    /// <see cref="TryMarkRoot"/>. <paramref name="pObjRef"/> is null for scratch registers Cosmos's
    /// REGDISPLAY does not track (skipped). An interior pointer (<c>GC_CALL_INTERIOR</c>: a byref or a
    /// span) is resolved to the object that contains it by <see cref="GetParentObject"/>, and marks
    /// nothing when it lies in no object; pinned (<c>GC_CALL_PINNED</c>) is a no-op for the non-moving
    /// mark phase.
    /// </summary>
    private static void PreciseRootTrampoline(void* ctx, nuint* pObjRef, uint gcRefFlags)
    {
        if (pObjRef == null)
        {
            return;
        }

        if ((gcRefFlags & GcRefFlags.GC_CALL_INTERIOR) != 0)
        {
            GCObject* parent = GetParentObject((byte*)*pObjRef);
            if (parent != null)
            {
                TryMarkRoot((nint)parent);
            }

            return;
        }

        TryMarkRoot((nint)(*pObjRef));
    }

    /// <summary>
    /// Resolves an interior pointer to the GC object that contains it.
    /// </summary>
    /// <remarks>
    /// The segment is found by address in both the SOH and the pinned lists. The slot's
    /// <c>GC_CALL_PINNED</c> flag cannot choose the list: it describes the stack slot (a <c>fixed</c>
    /// local), not the heap the object was allocated on, and the pinned sweep's free runs feed the
    /// shared free lists, so SOH TLABs can sit inside pinned segments.
    /// </remarks>
    /// <param name="interior">The pointer reported by the GCInfo decoder.</param>
    /// <returns>
    /// The containing object, or <c>null</c> when the pointer is outside every segment or lies in a
    /// free block, filler or unallocated space.
    /// </returns>
    private static GCObject* GetParentObject(byte* interior)
    {
        GCSegment* segment = s_segmentManager.GetSegmentContaining(interior);
        if (segment == null)
        {
            segment = s_pinnedSegmentManager.GetSegmentContaining(interior);
        }

        return segment != null ? FindObjectContaining(segment, interior) : null;
    }

    /// <summary>
    /// Walks <paramref name="segment"/> from its first object to the object whose extent covers
    /// <paramref name="interior"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The walk steps with the rules of <see cref="SweepSegment"/>, so the object it returns is one the
    /// sweep also visits as an object start and unmarks: a <see cref="FreeBlock"/> advances by its
    /// <see cref="FreeBlock.Size"/>, a header word that cannot be a MethodTable (zeroed TLAB gap,
    /// reserved header slot, stale heap pointer) by one pointer, and an object by its size rounded up
    /// the way the allocator rounds it. <see cref="GCObject.ComputeSize"/> alone is not a stride:
    /// strings and byte, char and short arrays have sizes that are not pointer multiples.
    /// </para>
    /// <para>
    /// It starts at <see cref="GCSegment.Start"/>, not at a brick-table entry. Objects allocated inside
    /// a TLAB, and TLABs refilled from the free list, are never recorded in the brick table, and its
    /// entries are never cleared when a sweep or a free-list refill reshapes the segment, so an entry
    /// can point inside a live object. The cost is one walk of one segment per precise interior root,
    /// and segments are sized for one TLAB refill or one large allocation.
    /// </para>
    /// </remarks>
    /// <param name="segment">The segment that contains <paramref name="interior"/>.</param>
    /// <param name="interior">The address to resolve.</param>
    /// <returns>The containing object, or <c>null</c> when no object covers the address.</returns>
    private static GCObject* FindObjectContaining(GCSegment* segment, byte* interior)
    {
        if (interior >= segment->Bump)
        {
            return null;
        }

        byte* ptr = segment->Start;
        while (ptr < segment->Bump)
        {
            GCObject* obj = (GCObject*)ptr;

            // Masked: the walk runs mid-mark, so objects already reached carry the mark bit.
            MethodTable* mt = obj->GetMethodTable();
            bool isObject = false;
            uint size;

            if (mt == s_freeMethodTable)
            {
                size = (uint)((FreeBlock*)ptr)->Size;
            }
            else if (mt == null || (ulong)mt < AddressSpace.KernelSpaceStart || IsInGCHeap((nint)mt))
            {
                size = (uint)sizeof(nint);
            }
            else
            {
                size = Align(obj->ComputeSize());
                isObject = true;
            }

            // The sweep stops at the same entry, so nothing past it is ever an object start.
            if (size == 0 || size > (uint)(segment->End - ptr))
            {
                return null;
            }

            if (interior < ptr + size)
            {
                return isObject ? obj : null;
            }

            ptr += size;
        }

        return null;
    }
}
