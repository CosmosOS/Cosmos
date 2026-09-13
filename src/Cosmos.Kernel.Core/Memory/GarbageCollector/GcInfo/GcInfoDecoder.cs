// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Port of TGcInfoDecoder from dotnet/runtime/src/coreclr/vm/gcinfodecoder.cpp (the decoder
// NativeAOT compiles with GCINFODECODER_NO_EE). Decodes a per-method GCInfo blob (GCINFO_VERSION 4)
// — header, safepoint table, slot lifetimes — and reports the live GC references at a code offset
// via EnumerateLiveSlots. The legacy (version < 4) / x86 / ReturnKind / PSPSym paths and the
// interruptibility queries the precise scan does not use are omitted. See issue #346.
//
// Cosmos's REGDISPLAY tracks only callee-saved registers, so GetRegisterSlot returns null for
// scratch registers and they are skipped — sound for the GC-triggering thread, which is always
// stopped at a call site, where GC refs are never live in caller-saved registers.

using Cosmos.Kernel.Core.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;

/// <summary>Decodes one method's GCInfo blob. Construct, then query / <see cref="EnumerateLiveSlots"/>.</summary>
internal unsafe struct GcInfoDecoder
{
    private const uint NO_STACK_BASE_REGISTER = 0xFFFFFFFF;

    private const int MAX_LINEAR_SEARCH = 32;

    /// <summary>Max slot-table entries the live-slot enumeration can handle without falling back.</summary>
    public const int MaxSlots = 256;

    private GcInfoBitStreamReader _reader;
    private uint _instructionOffset;
    private uint _version;

    private GcInfoHeaderFlags _headerFlags;
    private uint _codeLength;
    private uint _stackBaseRegister;
    private uint _numSafePoints;
    private uint _safePointIndex;
    private uint _numInterruptibleRanges;
    private uint _sizeOfStackOutgoingAndScratchArea;

    public GcInfoDecoder(byte* gcInfo, uint version, GcInfoDecoderFlags flags, uint instructionOffset)
    {
        _reader = new GcInfoBitStreamReader(gcInfo);
        _instructionOffset = instructionOffset;
        _version = version;
        _headerFlags = 0;
        _codeLength = 0;
        _stackBaseRegister = NO_STACK_BASE_REGISTER;
        _numSafePoints = 0;
        _safePointIndex = 0;
        _numInterruptibleRanges = 0;
        _sizeOfStackOutgoingAndScratchArea = 0;

        bool slimHeader = _reader.ReadOneFast() == 0;
        int remainingFlags = flags == GcInfoDecoderFlags.DECODE_EVERYTHING ? ~0 : (int)(uint)flags;

        if (!slimHeader)
        {
            if (PredecodeFatHeader(ref remainingFlags))
            {
                return;
            }
        }
        else
        {
            if (_reader.ReadOneFast() != 0)
            {
                _headerFlags = GcInfoHeaderFlags.GC_INFO_HAS_STACK_BASE_REGISTER;
                _stackBaseRegister = GcInfoEncoding.DenormalizeStackBaseRegister(0);
            }
            else
            {
                _headerFlags = 0;
                _stackBaseRegister = NO_STACK_BASE_REGISTER;
            }

            // v4: no ReturnKind in the slim header.
            remainingFlags &= ~(int)(GcInfoDecoderFlags.DECODE_RETURN_KIND | GcInfoDecoderFlags.DECODE_VARARG);
#if ARCH_ARM64
            remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_HAS_TAILCALLS;
#endif
            if (remainingFlags == 0)
            {
                return;
            }

            _codeLength = GcInfoEncoding.DenormalizeCodeLength((uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.CODE_LENGTH_ENCBASE));

            // The rest of the slim header has no encoded fields: no GS cookie, generics context,
            // edit-and-continue info or reverse-PInvoke frame, and a 0-byte outgoing/scratch area.
            remainingFlags &= ~(int)(GcInfoDecoderFlags.DECODE_CODE_LENGTH
                | GcInfoDecoderFlags.DECODE_PROLOG_LENGTH
                | GcInfoDecoderFlags.DECODE_GS_COOKIE
                | GcInfoDecoderFlags.DECODE_PSP_SYM
                | GcInfoDecoderFlags.DECODE_GENERICS_INST_CONTEXT
                | GcInfoDecoderFlags.DECODE_EDIT_AND_CONTINUE
                | GcInfoDecoderFlags.DECODE_REVERSE_PINVOKE_VAR);
            if (remainingFlags == 0)
            {
                return;
            }
        }

        _numSafePoints = (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.NUM_SAFE_POINTS_ENCBASE);
        _safePointIndex = _numSafePoints;

        _numInterruptibleRanges = slimHeader
            ? 0u
            : (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.NUM_INTERRUPTIBLE_RANGES_ENCBASE);

        if ((flags & GcInfoDecoderFlags.DECODE_GC_LIFETIMES) != 0 && _numSafePoints != 0)
        {
            _safePointIndex = FindSafePoint(_instructionOffset);
        }
    }

    /// <summary>The GCInfo format version this blob was decoded as (always <see cref="GcInfoEncoding.GCINFO_VERSION"/>).</summary>
    public readonly uint Version => _version;

    /// <summary>The method's code length — must equal the FDE's PC range.</summary>
    public readonly uint CodeLength => _codeLength;

#if ARCH_X64
    private readonly bool WantsReportOnlyLeaf => (_headerFlags & GcInfoHeaderFlags.GC_INFO_WANTS_REPORT_ONLY_LEAF) != 0;
#else
    private readonly bool WantsReportOnlyLeaf => false;
#endif

    // --- Header ---

    /// <summary>
    /// Pre-decode the fat header, advancing the bit stream past it. Returns true once everything
    /// <paramref name="remainingFlags"/> asked for has been decoded (the caller can stop). Header
    /// fields the precise GC scan does not consume — GS cookie / valid range, generics-instantiation
    /// context, edit-and-continue sizes, reverse-PInvoke frame slot — are decoded and discarded:
    /// the reads still have to happen to keep the stream aligned for the safepoint / slot tables.
    /// </summary>
    private bool PredecodeFatHeader(ref int remainingFlags)
    {
        _headerFlags = (GcInfoHeaderFlags)_reader.Read(GcInfoEncoding.GC_INFO_FLAGS_BIT_SIZE);

        // v4: no ReturnKind.
        remainingFlags &= ~(int)(GcInfoDecoderFlags.DECODE_RETURN_KIND | GcInfoDecoderFlags.DECODE_VARARG);
#if ARCH_ARM64
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_HAS_TAILCALLS;
#endif
        if (remainingFlags == 0)
        {
            return true;
        }

        _codeLength = GcInfoEncoding.DenormalizeCodeLength((uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.CODE_LENGTH_ENCBASE));
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_CODE_LENGTH;
        if (remainingFlags == 0)
        {
            return true;
        }

        // Prolog / epilog sizes (the GS-cookie valid range) — discarded.
        if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_GS_COOKIE) != 0)
        {
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.NORM_PROLOG_SIZE_ENCBASE);
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.NORM_EPILOG_SIZE_ENCBASE);
        }
        else if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_GENERICS_INST_CONTEXT_MASK) != GcInfoHeaderFlags.GC_INFO_HAS_GENERICS_INST_CONTEXT_NONE)
        {
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.NORM_PROLOG_SIZE_ENCBASE);
        }
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_PROLOG_LENGTH;
        if (remainingFlags == 0)
        {
            return true;
        }

        // GS-cookie stack slot — discarded.
        if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_GS_COOKIE) != 0)
        {
            _reader.DecodeVarLengthSigned(GcInfoEncoding.GS_COOKIE_STACK_SLOT_ENCBASE);
        }
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_GS_COOKIE;
        if (remainingFlags == 0)
        {
            return true;
        }

        // v4: no PSPSym in the header.
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_PSP_SYM;
        if (remainingFlags == 0)
        {
            return true;
        }

        // Generics-instantiation-context stack slot — discarded.
        if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_GENERICS_INST_CONTEXT_MASK) != GcInfoHeaderFlags.GC_INFO_HAS_GENERICS_INST_CONTEXT_NONE)
        {
            _reader.DecodeVarLengthSigned(GcInfoEncoding.GENERICS_INST_CONTEXT_STACK_SLOT_ENCBASE);
        }
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_GENERICS_INST_CONTEXT;
        if (remainingFlags == 0)
        {
            return true;
        }

        _stackBaseRegister = (_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_STACK_BASE_REGISTER) != 0
            ? GcInfoEncoding.DenormalizeStackBaseRegister((uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.STACK_BASE_REGISTER_ENCBASE))
            : NO_STACK_BASE_REGISTER;

        // Edit-and-continue preserved-area (+ ARM64 fixed-stack-frame) sizes — discarded.
        if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_HAS_EDIT_AND_CONTINUE_INFO) != 0)
        {
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.SIZE_OF_EDIT_AND_CONTINUE_PRESERVED_AREA_ENCBASE);
#if ARCH_ARM64
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.SIZE_OF_EDIT_AND_CONTINUE_FIXED_STACK_FRAME_ENCBASE);
#endif
        }
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_EDIT_AND_CONTINUE;
        if (remainingFlags == 0)
        {
            return true;
        }

        // Reverse-PInvoke frame stack slot — discarded.
        if ((_headerFlags & GcInfoHeaderFlags.GC_INFO_REVERSE_PINVOKE_FRAME) != 0)
        {
            _reader.DecodeVarLengthSigned(GcInfoEncoding.REVERSE_PINVOKE_FRAME_ENCBASE);
        }
        remainingFlags &= ~(int)GcInfoDecoderFlags.DECODE_REVERSE_PINVOKE_VAR;
        if (remainingFlags == 0)
        {
            return true;
        }

        _sizeOfStackOutgoingAndScratchArea = GcInfoEncoding.DenormalizeSizeOfStackArea((uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.SIZE_OF_STACK_AREA_ENCBASE));
        return false;
    }

    // --- Safepoints ---

    private uint NarrowSafePointSearch(ulong savedPos, uint breakOffsetNorm, out uint searchEnd)
    {
        int low = 0;
        int high = (int)_numSafePoints;
        int numBitsPerOffset = GcInfoEncoding.CeilOfLog2(GcInfoEncoding.NormalizeCodeOffset(_codeLength));
        while (high - low > MAX_LINEAR_SEARCH)
        {
            int mid = (low + high) / 2;
            _reader.SetCurrentPos(savedPos + (ulong)((uint)mid * (uint)numBitsPerOffset));
            uint midSpOffset = (uint)_reader.Read(numBitsPerOffset);
            if (breakOffsetNorm < midSpOffset)
            {
                high = mid;
            }
            else
            {
                low = mid;
            }
        }
        _reader.SetCurrentPos(savedPos + (ulong)((uint)low * (uint)numBitsPerOffset));
        searchEnd = (uint)high;
        return (uint)low;
    }

    /// <summary>Find the safepoint index for <paramref name="breakOffset"/>, or <see cref="_numSafePoints"/> if none. Leaves the reader positioned just past the safepoint table.</summary>
    private uint FindSafePoint(uint breakOffset)
    {
        uint result = _numSafePoints;
        ulong savedPos = _reader.GetCurrentPos();
        int numBitsPerOffset = GcInfoEncoding.CeilOfLog2(GcInfoEncoding.NormalizeCodeOffset(_codeLength));
        uint normBreakOffset = GcInfoEncoding.NormalizeCodeOffset(breakOffset);

        uint linearSearchStart = 0;
        uint linearSearchEnd = _numSafePoints;
        if (linearSearchEnd - linearSearchStart > MAX_LINEAR_SEARCH)
        {
            linearSearchStart = NarrowSafePointSearch(savedPos, normBreakOffset, out linearSearchEnd);
        }

        for (uint i = linearSearchStart; i < linearSearchEnd; i++)
        {
            uint spOffset = (uint)_reader.Read(numBitsPerOffset);
            if (spOffset == normBreakOffset)
            {
                result = i;
                break;
            }
            if (spOffset > normBreakOffset)
            {
                break;
            }
        }

        // Position exactly at the end of the safepoint table (Skip handles a possible stream end).
        _reader.Skip((long)(savedPos + (ulong)_numSafePoints * (ulong)numBitsPerOffset) - (long)_reader.GetCurrentPos());
        return result;
    }

    /// <summary>
    /// Total number of slot-table entries (registers + tracked stack + untracked stack). Consumes the
    /// reader; valid only on a decoder freshly constructed with <see cref="GcInfoDecoderFlags.DECODE_GC_LIFETIMES"/>.
    /// Used by the GCInfo decoder validation test.
    /// </summary>
    public uint GetGcSlotCount()
    {
        for (uint i = 0; i < _numInterruptibleRanges; i++)
        {
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA1_ENCBASE);
            _reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA2_ENCBASE);
        }
        GcSlotDesc* buf = stackalloc GcSlotDesc[MaxSlots];
        GcSlotTable table = default;
        table.DecodeSlotTable(ref _reader, buf, MaxSlots);
        return table.NumSlots;
    }

    // --- Live-slot enumeration ---

    /// <summary>
    /// Report every live GC reference at the decoder's code offset to <paramref name="cb"/>.
    /// Returns false if the slot table did not fit (<see cref="MaxSlots"/>); the caller should then
    /// fall back to a conservative scan of this frame. Otherwise returns true.
    /// </summary>
    public bool EnumerateLiveSlots(REGDISPLAY* pRD, bool reportScratchSlots, CodeManagerFlags inputFlags, delegate*<void*, nuint*, uint, void> cb, void* ctx)
    {
        bool executionAborted = (inputFlags & CodeManagerFlags.ExecutionAborted) != 0;

        if (WantsReportOnlyLeaf && (inputFlags & CodeManagerFlags.ParentOfFuncletStackFrame) != 0)
        {
            return true;
        }

        GcSlotDesc* slotsBuf = stackalloc GcSlotDesc[MaxSlots];
        GcSlotTable slotDecoder = default;

        uint normBreakOffset = GcInfoEncoding.NormalizeCodeOffset(_instructionOffset);
        uint pseudoBreakOffset = 0;
        uint numInterruptibleLength = 0;

        if (_safePointIndex < _numSafePoints && !executionAborted)
        {
            // We're at a known call site — skip the interruptibility information.
            for (uint i = 0; i < _numInterruptibleRanges; i++)
            {
                _reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA1_ENCBASE);
                _reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA2_ENCBASE);
            }
        }
        else
        {
            // Not at a known safepoint: either inside a fully-interruptible range (handled below),
            // or a MinOpts method with only untracked refs and no ranges (nothing to read here).
            if (_numInterruptibleRanges != 0)
            {
                int countIntersections = 0;
                uint lastNormStop = 0;
                for (uint i = 0; i < _numInterruptibleRanges; i++)
                {
                    uint normStartDelta = (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA1_ENCBASE);
                    uint normStopDelta = (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.INTERRUPTIBLE_RANGE_DELTA2_ENCBASE) + 1;
                    uint normStart = lastNormStop + normStartDelta;
                    uint normStop = normStart + normStopDelta;
                    if (normBreakOffset >= normStart && normBreakOffset < normStop)
                    {
                        countIntersections++;
                        pseudoBreakOffset = numInterruptibleLength + normBreakOffset - normStart;
                    }
                    numInterruptibleLength += normStopDelta;
                    lastNormStop = normStop;
                }
                if (countIntersections == 0)
                {
                    // Aborted (or otherwise unreachable) and not fully interruptible — report nothing.
                    goto ExitSuccess;
                }
            }
        }

        // --- Slot table ---
        slotDecoder.DecodeSlotTable(ref _reader, slotsBuf, MaxSlots);
        if (slotDecoder.Overflowed)
        {
            return false;
        }

        {
            uint numSlots = slotDecoder.NumTracked;
            if (numSlots == 0)
            {
                goto ReportUntracked;
            }

            uint numBitsPerOffsetSlots = 0;
            if (_numSafePoints > 0 && _reader.ReadOneFast() != 0)
            {
                numBitsPerOffsetSlots = (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.POINTER_SIZE_ENCBASE) + 1;
            }

            if (!executionAborted && _safePointIndex != _numSafePoints)
            {
                // --- Partially interruptible: we're at a safepoint ---
                if (numBitsPerOffsetSlots != 0)
                {
                    ulong offsetTablePos = _reader.GetCurrentPos();
                    _reader.Skip((long)_safePointIndex * numBitsPerOffsetSlots);
                    ulong liveStatesOffset = _reader.Read((int)numBitsPerOffsetSlots);
                    ulong liveStatesStart = (offsetTablePos + (ulong)_numSafePoints * numBitsPerOffsetSlots + 7) & ~(ulong)7;
                    _reader.SetCurrentPos(liveStatesStart + liveStatesOffset);
                    if (_reader.ReadOneFast() != 0)
                    {
                        // RLE-encoded live state.
                        bool fSkip = _reader.ReadOneFast() == 0;
                        bool fReport = true;
                        uint readSlots = (uint)_reader.DecodeVarLengthUnsigned(fSkip ? GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE : GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE);
                        fSkip = !fSkip;
                        while (readSlots < numSlots)
                        {
                            uint cnt = (uint)_reader.DecodeVarLengthUnsigned(fSkip ? GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE : GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE) + 1;
                            if (fReport)
                            {
                                for (uint slotIndex = readSlots; slotIndex < readSlots + cnt; slotIndex++)
                                {
                                    ReportSlotToGC(ref slotDecoder, slotIndex, pRD, reportScratchSlots, inputFlags, cb, ctx);
                                }
                            }
                            readSlots += cnt;
                            fSkip = !fSkip;
                            fReport = !fReport;
                        }
                        goto ReportUntracked;
                    }
                    // Otherwise a plain 1-bit-per-slot live state — handled by the loop below.
                }
                else
                {
                    _reader.Skip((long)_safePointIndex * numSlots);
                }

                for (uint slotIndex = 0; slotIndex < numSlots; slotIndex++)
                {
                    if (_reader.ReadOneFast() != 0)
                    {
                        ReportSlotToGC(ref slotDecoder, slotIndex, pRD, reportScratchSlots, inputFlags, cb, ctx);
                    }
                }
                goto ReportUntracked;
            }
            else
            {
                _reader.Skip((long)_numSafePoints * numSlots);
                if (_numInterruptibleRanges == 0)
                {
                    goto ReportUntracked;
                }
            }

            // --- Fully interruptible: arbitrary IP inside an interruptible range ---
            uint numChunks = (numInterruptibleLength + GcInfoEncoding.NUM_NORM_CODE_OFFSETS_PER_CHUNK - 1) / GcInfoEncoding.NUM_NORM_CODE_OFFSETS_PER_CHUNK;
            uint breakChunk = pseudoBreakOffset / GcInfoEncoding.NUM_NORM_CODE_OFFSETS_PER_CHUNK;

            uint numBitsPerPointer = (uint)_reader.DecodeVarLengthUnsigned(GcInfoEncoding.POINTER_SIZE_ENCBASE);
            if (numBitsPerPointer == 0)
            {
                goto ReportUntracked;
            }

            ulong pointerTablePos = _reader.GetCurrentPos();
            ulong chunkPointer;
            uint chunk = breakChunk;
            for (; ; )
            {
                _reader.SetCurrentPos(pointerTablePos + (ulong)chunk * numBitsPerPointer);
                chunkPointer = _reader.Read((int)numBitsPerPointer);
                if (chunkPointer != 0)
                {
                    break;
                }
                if (chunk == 0)
                {
                    goto ReportUntracked;
                }
                chunk--;
            }

            ulong chunksStartPos = (pointerTablePos + (ulong)numChunks * numBitsPerPointer + 7) & ~(ulong)7;
            ulong chunkPos = chunksStartPos + chunkPointer - 1;
            _reader.SetCurrentPos(chunkPos);

            {
                GcInfoBitStreamReader couldBeLiveReader = _reader;

                uint numCouldBeLiveSlots = 0;
                if (_reader.ReadOneFast() != 0)
                {
                    // RLE-encoded "could be live" bit vector.
                    bool fSkip = _reader.ReadOneFast() == 0;
                    bool fReport = true;
                    uint readSlots = (uint)_reader.DecodeVarLengthUnsigned(fSkip ? GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE : GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE);
                    fSkip = !fSkip;
                    while (readSlots < numSlots)
                    {
                        uint cnt = (uint)_reader.DecodeVarLengthUnsigned(fSkip ? GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE : GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE) + 1;
                        if (fReport)
                        {
                            numCouldBeLiveSlots += cnt;
                        }
                        readSlots += cnt;
                        fSkip = !fSkip;
                        fReport = !fReport;
                    }
                }
                else
                {
                    for (uint i = 0; i < numSlots; i++)
                    {
                        if (_reader.ReadOneFast() != 0)
                        {
                            numCouldBeLiveSlots++;
                        }
                    }
                }

                GcInfoBitStreamReader finalStateReader = _reader;
                _reader.Skip(numCouldBeLiveSlots);

                uint slotIndex = 0;
                bool fSimple = couldBeLiveReader.ReadOneFast() == 0;
                bool fSkipFirst = false;
                uint cntRun = 0;
                if (!fSimple)
                {
                    fSkipFirst = couldBeLiveReader.ReadOneFast() == 0;
                    slotIndex = unchecked((uint)-1);
                }
                for (uint i = 0; i < numCouldBeLiveSlots; i++)
                {
                    if (fSimple)
                    {
                        while (couldBeLiveReader.ReadOneFast() == 0)
                        {
                            slotIndex++;
                        }
                    }
                    else if (cntRun > 0)
                    {
                        cntRun--;
                    }
                    else if (fSkipFirst)
                    {
                        uint tmp = (uint)couldBeLiveReader.DecodeVarLengthUnsigned(GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE) + 1;
                        slotIndex += tmp;
                        cntRun = (uint)couldBeLiveReader.DecodeVarLengthUnsigned(GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE);
                    }
                    else
                    {
                        uint tmp = (uint)couldBeLiveReader.DecodeVarLengthUnsigned(GcInfoEncoding.LIVESTATE_RLE_RUN_ENCBASE) + 1;
                        slotIndex += tmp;
                        cntRun = (uint)couldBeLiveReader.DecodeVarLengthUnsigned(GcInfoEncoding.LIVESTATE_RLE_SKIP_ENCBASE);
                    }

                    uint isLive = (uint)finalStateReader.Read(1);

                    if (chunk == breakChunk)
                    {
                        uint normBreakOffsetDelta = pseudoBreakOffset % GcInfoEncoding.NUM_NORM_CODE_OFFSETS_PER_CHUNK;
                        for (; ; )
                        {
                            if (_reader.ReadOneFast() == 0)
                            {
                                break;
                            }
                            uint transitionOffset = (uint)_reader.Read(GcInfoEncoding.NUM_NORM_CODE_OFFSETS_PER_CHUNK_LOG2);
                            if (transitionOffset > normBreakOffsetDelta)
                            {
                                isLive ^= 1;
                            }
                        }
                    }

                    if (isLive != 0)
                    {
                        ReportSlotToGC(ref slotDecoder, slotIndex, pRD, reportScratchSlots, inputFlags, cb, ctx);
                    }

                    slotIndex++;
                }
            }
        }

    ReportUntracked:
        if (slotDecoder.NumUntracked != 0 && (inputFlags & (CodeManagerFlags.ParentOfFuncletStackFrame | CodeManagerFlags.NoReportUntracked)) == 0)
        {
            ReportUntrackedSlots(ref slotDecoder, pRD, inputFlags, cb, ctx);
        }

    ExitSuccess:
        return true;
    }

    private readonly void ReportUntrackedSlots(ref GcSlotTable slotDecoder, REGDISPLAY* pRD, CodeManagerFlags inputFlags, delegate*<void*, nuint*, uint, void> cb, void* ctx)
    {
        for (uint slotIndex = slotDecoder.NumTracked; slotIndex < slotDecoder.NumSlots; slotIndex++)
        {
            ReportSlotToGC(ref slotDecoder, slotIndex, pRD, true, inputFlags, cb, ctx);
        }
    }

    private readonly void ReportSlotToGC(ref GcSlotTable slotDecoder, uint slotIndex, REGDISPLAY* pRD, bool reportScratchSlots, CodeManagerFlags inputFlags, delegate*<void*, nuint*, uint, void> cb, void* ctx)
    {
        ref GcSlotDesc slot = ref slotDecoder[slotIndex];
        bool reportFpBasedSlotsOnly = (inputFlags & CodeManagerFlags.ReportFPBasedSlotsOnly) != 0;

        if (slotIndex < slotDecoder.NumRegisters)
        {
            int regNum = (int)slot.RegisterNumber;
            if ((reportScratchSlots || !IsScratchRegister(regNum)) && !reportFpBasedSlotsOnly)
            {
                ReportRegisterToGC(regNum, (uint)slot.Flags, pRD, cb, ctx);
            }
        }
        else
        {
            int spOffset = slot.SpOffset;
            GcStackSlotBase spBase = slot.StackBase;
            if ((reportScratchSlots || !IsScratchStackSlot(spOffset, spBase, pRD))
                && (!reportFpBasedSlotsOnly || spBase == GcStackSlotBase.GC_FRAMEREG_REL))
            {
                ReportStackSlotToGC(spOffset, spBase, (uint)slot.Flags, pRD, cb, ctx);
            }
        }
    }

    private readonly void ReportRegisterToGC(int regNum, uint gcFlags, REGDISPLAY* pRD, delegate*<void*, nuint*, uint, void> cb, void* ctx)
    {
        nuint* pObjRef = GetRegisterSlot(regNum, pRD);
        if (pObjRef == null)
        {
            // Scratch register Cosmos's REGDISPLAY does not track — see file header.
            return;
        }
        cb(ctx, pObjRef, gcFlags);
    }

    private readonly void ReportStackSlotToGC(int spOffset, GcStackSlotBase spBase, uint gcFlags, REGDISPLAY* pRD, delegate*<void*, nuint*, uint, void> cb, void* ctx)
    {
        nuint* pObjRef = GetStackSlot(spOffset, spBase, pRD);
        if (pObjRef == null)
        {
            return;
        }
        cb(ctx, pObjRef, gcFlags);
    }

    private readonly nuint* GetStackSlot(int spOffset, GcStackSlotBase spBase, REGDISPLAY* pRD)
    {
        if (spBase == GcStackSlotBase.GC_SP_REL)
        {
            return (nuint*)((byte*)pRD->SP + spOffset);
        }
        if (spBase == GcStackSlotBase.GC_CALLER_SP_REL)
        {
            // GET_CALLER_SP is 0 for NativeAOT — ILC never emits caller-SP-relative slots.
            return null;
        }
        // GC_FRAMEREG_REL
        if (_stackBaseRegister == NO_STACK_BASE_REGISTER)
        {
            return null;
        }
        nuint* pFrameReg = GetRegisterSlot((int)_stackBaseRegister, pRD);
        if (pFrameReg == null)
        {
            return null;
        }
        return (nuint*)((byte*)*pFrameReg + spOffset);
    }

    private readonly bool IsScratchStackSlot(int spOffset, GcStackSlotBase spBase, REGDISPLAY* pRD)
    {
        nuint* pSlot = GetStackSlot(spOffset, spBase, pRD);
        if (pSlot == null)
        {
            return false;
        }
        return (nuint)pSlot < pRD->SP + _sizeOfStackOutgoingAndScratchArea;
    }

#if ARCH_X64
    private static nuint* GetRegisterSlot(int regNum, REGDISPLAY* pRD)
    {
        // regNum is the raw GcInfo x64 register number: 0=rax,1=rcx,2=rdx,3=rbx,4=rsp(unused),5=rbp,6=rsi,7=rdi,8..15=r8..r15.
        // Mirror gcinfodecoder.cpp: NativeAOT's RegDisplay omits rsp, so registers above it shift down by one.
        if (regNum > 4)
        {
            regNum--;
        }
        // After the shift: 3=rbx,4=rbp,5=rsi,6=rdi,7..10=r8..r11,11..14=r12..r15.
        // Cosmos's REGDISPLAY only tracks callee-saved registers (rbx, rbp, rsi, rdi, r12-r15).
        switch (regNum)
        {
            case 3: return pRD->pRbx;
            case 4: return pRD->pRbp;
            case 5: return pRD->pRsi;
            case 6: return pRD->pRdi;
            case 11: return pRD->pR12;
            case 12: return pRD->pR13;
            case 13: return pRD->pR14;
            case 14: return pRD->pR15;
            default: return null;
        }
    }

    private static bool IsScratchRegister(int regNum)
    {
        // SysV / UNIX_AMD64_ABI preserved (callee-saved) set: rbx(3), rbp(5), r12(12)..r15(15).
        const uint PreservedRegMask = (1u << 3) | (1u << 5) | (1u << 12) | (1u << 13) | (1u << 14) | (1u << 15);
        return (PreservedRegMask & (1u << regNum)) == 0;
    }
#elif ARCH_ARM64
    private static nuint* GetRegisterSlot(int regNum, REGDISPLAY* pRD)
    {
        // GcInfo ARM64 register numbering: 0=X0..30=X30 (18=TEB, never reported).
        // Cosmos's REGDISPLAY only stores callee-saved values (X19-X28, X29/FP, X30/LR).
        switch (regNum)
        {
            case 19: return &pRD->X19;
            case 20: return &pRD->X20;
            case 21: return &pRD->X21;
            case 22: return &pRD->X22;
            case 23: return &pRD->X23;
            case 24: return &pRD->X24;
            case 25: return &pRD->X25;
            case 26: return &pRD->X26;
            case 27: return &pRD->X27;
            case 28: return &pRD->X28;
            case 29: return &pRD->FP;
            case 30: return &pRD->LR;
            default: return null;
        }
    }

    private static bool IsScratchRegister(int regNum) => regNum <= 17 || regNum >= 29;
#endif
}
