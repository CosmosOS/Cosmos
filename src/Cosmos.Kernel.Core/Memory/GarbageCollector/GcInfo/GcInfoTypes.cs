// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Constants/enums for the NativeAOT GCInfo format (GCINFO_VERSION 4), ported from
// dotnet/runtime/src/coreclr/inc/{gcinfotypes.h,gcinfodecoder.h}. See issue #346.
//
// Only the subset needed by the precise GC stack scanner is ported. Architecture-specific
// encoding parameters are selected via ARCH_X64 / ARCH_ARM64 (mirrors AMD64GcInfoEncoding /
// ARM64GcInfoEncoding in gcinfotypes.h).

using System;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;

/// <summary>Flags controlling which parts of a GCInfo blob the decoder pre-decodes.</summary>
[Flags]
internal enum GcInfoDecoderFlags : uint
{
    DECODE_EVERYTHING = 0x0,
    DECODE_SECURITY_OBJECT = 0x01,
    DECODE_CODE_LENGTH = 0x02,
    DECODE_VARARG = 0x04,
    DECODE_GC_LIFETIMES = 0x10,
    DECODE_NO_VALIDATION = 0x20,
    DECODE_PSP_SYM = 0x40,    // unused starting with v4
    DECODE_GENERICS_INST_CONTEXT = 0x80,
    DECODE_GS_COOKIE = 0x100,
    DECODE_PROLOG_LENGTH = 0x400,
    DECODE_EDIT_AND_CONTINUE = 0x800,
    DECODE_REVERSE_PINVOKE_VAR = 0x1000,
    DECODE_RETURN_KIND = 0x2000,  // unused starting with v4
    DECODE_HAS_TAILCALLS = 0x4000,  // ARM/ARM64 only
}

/// <summary>Flags stored at the start of a (fat) GCInfo header.</summary>
[Flags]
internal enum GcInfoHeaderFlags : uint
{
    GC_INFO_HAS_GS_COOKIE = 0x4,
    GC_INFO_HAS_GENERICS_INST_CONTEXT_MASK = 0x30,
    GC_INFO_HAS_GENERICS_INST_CONTEXT_NONE = 0x00,
    GC_INFO_HAS_GENERICS_INST_CONTEXT_MT = 0x10,
    GC_INFO_HAS_GENERICS_INST_CONTEXT_MD = 0x20,
    GC_INFO_HAS_GENERICS_INST_CONTEXT_THIS = 0x30,
    GC_INFO_HAS_STACK_BASE_REGISTER = 0x40,
#if ARCH_X64
    GC_INFO_WANTS_REPORT_ONLY_LEAF = 0x80,
#elif ARCH_ARM64
    GC_INFO_HAS_TAILCALLS = 0x80,
#endif
    GC_INFO_HAS_EDIT_AND_CONTINUE_INFO = 0x100,
    GC_INFO_REVERSE_PINVOKE_FRAME = 0x200,
}

/// <summary>Per-slot flags decoded from the 2-bit field in the slot table (matches GcSlotFlags / GC_CALL_*).</summary>
[Flags]
internal enum GcSlotFlags
{
    GC_SLOT_BASE = 0x0,
    GC_SLOT_INTERIOR = 0x1,   // == GC_CALL_INTERIOR
    GC_SLOT_PINNED = 0x2,   // == GC_CALL_PINNED
    GC_SLOT_UNTRACKED = 0x4,
}

/// <summary>Stack-slot base register kind (matches GcStackSlotBase).</summary>
internal enum GcStackSlotBase
{
    GC_CALLER_SP_REL = 0x0,
    GC_SP_REL = 0x1,
    GC_FRAMEREG_REL = 0x2,
}

/// <summary>Per-frame flags passed to <see cref="GcInfoDecoder.EnumerateLiveSlots"/> (subset of ICodeManagerFlags).</summary>
[Flags]
internal enum CodeManagerFlags : uint
{
    None = 0,
    ActiveStackFrame = 0x0001, // currently-active (leaf) frame
    ExecutionAborted = 0x0002, // execution will not resume here
    ParentOfFuncletStackFrame = 0x0040,
    NoReportUntracked = 0x0080, // do not report untracked slots (filters)
    ReportFPBasedSlotsOnly = 0x0200,
}

/// <summary>GC reference flags reported to the enum callback (matches GC_CALL_INTERIOR / GC_CALL_PINNED).</summary>
internal static class GcRefFlags
{
    public const uint GC_CALL_INTERIOR = 0x1;
    public const uint GC_CALL_PINNED = 0x2;
}

/// <summary>One decoded slot-table entry: a register, or a (base,offset) stack location.</summary>
internal struct GcSlotDesc
{
    public uint RegisterNumber;     // valid when IsRegister
    public int SpOffset;            // valid when !IsRegister; denormalized (bytes)
    public GcStackSlotBase StackBase;
    public GcSlotFlags Flags;       // 0..3: combination of GC_SLOT_INTERIOR | GC_SLOT_PINNED
    public bool IsRegister;
}

// The live-slot enumeration reports each GC reference through an allocation-free function pointer:
//   delegate*<void* context, nuint* pObjRef, uint gcRefFlags, void>
// pObjRef points at the slot holding the reference (the callback must not dereference it blindly);
// it may be null for a scratch register Cosmos's REGDISPLAY does not track. See GcInfoDecoder.EnumerateLiveSlots.

/// <summary>
/// Architecture-specific GCInfo encoding parameters (the AMD64GcInfoEncoding / ARM64GcInfoEncoding
/// constants from gcinfotypes.h). Format version is fixed at <see cref="GCINFO_VERSION"/>.
/// </summary>
internal static class GcInfoEncoding
{
    /// <summary>The GCInfo format version this decoder understands (GCINFO_VERSION in gcinfo.h).</summary>
    public const uint GCINFO_VERSION = 4;

    public const int GC_INFO_FLAGS_BIT_SIZE = 10;

    public const uint NUM_NORM_CODE_OFFSETS_PER_CHUNK = 64;
    public const int NUM_NORM_CODE_OFFSETS_PER_CHUNK_LOG2 = 6;

#if ARCH_X64
    private const int STACK_SLOT_NORM_SHIFT = 3;            // GC pointers are 8-byte aligned
    private const int CODE_LENGTH_NORM_SHIFT = 0;          // x64 instructions are byte-granular
    private const int CODE_OFFSET_NORM_SHIFT = 0;
    private const int SIZE_OF_STACK_AREA_NORM_SHIFT = 3;
    public const uint STACK_BASE_REGISTER_XOR = 5;         // RBP encoded as 0

    public const int CODE_LENGTH_ENCBASE = 8;
    public const int STACK_BASE_REGISTER_ENCBASE = 3;
    public const int NUM_REGISTERS_ENCBASE = 2;
    public const int NUM_STACK_SLOTS_ENCBASE = 2;
    public const int NUM_SAFE_POINTS_ENCBASE = 2;
#elif ARCH_ARM64
    private const int STACK_SLOT_NORM_SHIFT = 3;
    private const int CODE_LENGTH_NORM_SHIFT = 2;          // ARM64 instructions are 4 bytes
    private const int CODE_OFFSET_NORM_SHIFT = 2;
    private const int SIZE_OF_STACK_AREA_NORM_SHIFT = 3;
    public const uint STACK_BASE_REGISTER_XOR = 29;        // X29/FP encoded as 0

    public const int CODE_LENGTH_ENCBASE = 8;
    public const int STACK_BASE_REGISTER_ENCBASE = 2;
    public const int NUM_REGISTERS_ENCBASE = 3;
    public const int NUM_STACK_SLOTS_ENCBASE = 2;
    public const int NUM_SAFE_POINTS_ENCBASE = 3;
    public const int SIZE_OF_EDIT_AND_CONTINUE_FIXED_STACK_FRAME_ENCBASE = 4;
#else
#error "GcInfoDecoder supports ARCH_X64 and ARCH_ARM64 only"
#endif

    // Encoding bases shared by AMD64 and ARM64.
    public const int PSP_SYM_STACK_SLOT_ENCBASE = 6;
    public const int GENERICS_INST_CONTEXT_STACK_SLOT_ENCBASE = 6;
    public const int GS_COOKIE_STACK_SLOT_ENCBASE = 6;
    public const int SIZE_OF_EDIT_AND_CONTINUE_PRESERVED_AREA_ENCBASE = 4;
    public const int REVERSE_PINVOKE_FRAME_ENCBASE = 6;
    public const int SIZE_OF_STACK_AREA_ENCBASE = 3;
    public const int NUM_UNTRACKED_SLOTS_ENCBASE = 1;
    public const int NUM_INTERRUPTIBLE_RANGES_ENCBASE = 1;
    public const int NORM_PROLOG_SIZE_ENCBASE = 5;
    public const int NORM_EPILOG_SIZE_ENCBASE = 3;
    public const int INTERRUPTIBLE_RANGE_DELTA1_ENCBASE = 6;
    public const int INTERRUPTIBLE_RANGE_DELTA2_ENCBASE = 6;
    public const int REGISTER_ENCBASE = 3;
    public const int REGISTER_DELTA_ENCBASE = 2;
    public const int STACK_SLOT_ENCBASE = 6;
    public const int STACK_SLOT_DELTA_ENCBASE = 4;
    public const int POINTER_SIZE_ENCBASE = 3;
    public const int LIVESTATE_RLE_RUN_ENCBASE = 2;
    public const int LIVESTATE_RLE_SKIP_ENCBASE = 4;

    public static int NormalizeStackSlot(int x) => x >> STACK_SLOT_NORM_SHIFT;
    public static int DenormalizeStackSlot(int x) => x << STACK_SLOT_NORM_SHIFT;
    public static uint NormalizeCodeLength(uint x) => x >> CODE_LENGTH_NORM_SHIFT;
    public static uint DenormalizeCodeLength(uint x) => x << CODE_LENGTH_NORM_SHIFT;
    public static uint NormalizeCodeOffset(uint x) => x >> CODE_OFFSET_NORM_SHIFT;
    public static uint DenormalizeCodeOffset(uint x) => x << CODE_OFFSET_NORM_SHIFT;
    public static uint NormalizeSizeOfStackArea(uint x) => x >> SIZE_OF_STACK_AREA_NORM_SHIFT;
    public static uint DenormalizeSizeOfStackArea(uint x) => x << SIZE_OF_STACK_AREA_NORM_SHIFT;
    public static uint NormalizeStackBaseRegister(uint x) => x ^ STACK_BASE_REGISTER_XOR;
    public static uint DenormalizeStackBaseRegister(uint x) => x ^ STACK_BASE_REGISTER_XOR;

    /// <summary>ceil(log2(x)) for x &gt; 0 — bits needed to represent the values 0..x-1.</summary>
    public static int CeilOfLog2(ulong x)
    {
        // Matches CeilOfLog2 in gcinfotypes.h: BitScanReverse((x << 1) - 1).
        x = (x << 1) - 1;
        int result = 0;
        while ((x >>= 1) != 0)
        {
            result++;
        }
        return result;
    }
}
