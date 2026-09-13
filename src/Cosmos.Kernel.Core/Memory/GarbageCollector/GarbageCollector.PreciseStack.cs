// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;
using Cosmos.Kernel.Core.Runtime;
using Cosmos.Kernel.Core.Runtime.GcInfo;

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

        GcInfoDecoder decoder = new GcInfoDecoder(mi.GcInfo, GcInfoEncoding.GCINFO_VERSION, GcInfoDecoderFlags.DECODE_GC_LIFETIMES, mi.CodeOffset);
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
    /// REGDISPLAY does not track (skipped). An interior pointer (<c>GC_CALL_INTERIOR</c>) is passed
    /// through as-is — <see cref="TryMarkRoot"/>'s MethodTable check then drops a non-header byref,
    /// the same hole the conservative scanner has; pinned (<c>GC_CALL_PINNED</c>) is a no-op for the
    /// non-moving mark phase.
    /// </summary>
    private static void PreciseRootTrampoline(void* ctx, nuint* pObjRef, uint gcRefFlags)
    {
        if (pObjRef == null)
        {
            return;
        }

        if ((gcRefFlags & GcRefFlags.GC_CALL_INTERIOR) != 0)
        {
            // An interior pointer is reported. Find the parent object in the GC heap and mark it.
            // Check if we are dealing with a pinned object, we need to use the correct segment list.
            void* obj = (gcRefFlags & GcRefFlags.GC_CALL_PINNED) != 0
                        ? GetParentObject((void*)*pObjRef, s_pinnedSegmentManager.Segments)
                        : GetParentObject((void*)*pObjRef, s_segmentManager.Segments);

            TryMarkRoot((nint)obj);
            return;
        }
        TryMarkRoot((nint)(*pObjRef));
    }

    private static void* GetParentObject(void* obj, GCSegment* s_segments)
    {
        var segment = s_segments;

        while (segment != null)
        {
            if (obj >= segment->Start && obj < segment->End)
            {
                var start = segment->FindClosestObjectBelow((nint)obj);

                var segEnum = new GCSegment.Enumerator((byte*)start, (byte*)obj);

                while (segEnum.MoveNext())
                {
                    var current = segEnum.Current;
                    if (current == obj)
                    {
                        return current;
                    }

                    if (obj > current && obj < (byte*)current + current->ComputeSize())
                    {
                        return current;
                    }
                }
            }
            segment = segment->Next;
        }

        return obj;
    }
}
