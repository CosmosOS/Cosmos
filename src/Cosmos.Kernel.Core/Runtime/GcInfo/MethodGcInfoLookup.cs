// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
//
// Resolves an instruction pointer to the corresponding method's GCInfo blob and exposes the DWARF
// .eh_frame walk results (CIE pointer, FDE-instruction range, LSDA pointer) every other CFI
// consumer in the kernel needs. Mirrors UnixNativeCodeManager::FindMethodInfo + GetCodeOffset
// (dotnet/runtime/src/coreclr/nativeaot/Runtime/unix/UnixNativeCodeManager.cpp). See issue #346.
//
// This is the single .eh_frame / LSDA-header parser in the kernel:
// - ExceptionHelper.TryGetMethodLSDA delegates here (handler lookup).
// - ExceptionHelper.UnwindOneFrameWithCFI delegates here (CFI unwinding — needs the CIE + FDE
//   instruction range, returned via TryGetMethodCFI).
// - The precise GC stack scan uses TryGetMethodGcInfo.

using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Runtime.GcInfo;

/// <summary>Maps an instruction pointer to its method's GCInfo blob, LSDA, and CFI data.</summary>
internal static unsafe class MethodGcInfoLookup
{
    // LSDA "unwind block" flags (UnixNativeCodeManager.cpp).
    public const byte UBF_FUNC_KIND_MASK = 0x03;
    public const byte UBF_FUNC_KIND_ROOT = 0x00;
    public const byte UBF_FUNC_KIND_FILTER = 0x02;
    public const byte UBF_FUNC_HAS_EHINFO = 0x04;
    public const byte UBF_FUNC_HAS_ASSOCIATED_DATA = 0x10;

    /// <summary>Resolved GCInfo location for an instruction pointer.</summary>
    public struct MethodGcInfo
    {
        /// <summary>Pointer to the GCInfo blob (into .dotnet_eh_table).</summary>
        public byte* GcInfo;
        /// <summary>ip minus the main method's start address (the offset GCInfo is indexed by).</summary>
        public uint CodeOffset;
        /// <summary>FDE PC-begin for the (sub)function containing ip — the funclet start for a funclet.</summary>
        public nuint MethodStart;
        /// <summary>FDE PC-end for that (sub)function.</summary>
        public nuint MethodEnd;
        public bool IsFunclet;
        public bool IsFilter;
    }

    /// <summary>
    /// Raw <c>.eh_frame</c> walk result for an instruction pointer — everything callers need to
    /// drive a DWARF CFI unwind: the FDE's PC range, its LSDA pointer, the CIE it references, and
    /// the FDE-instruction range.
    /// </summary>
    internal struct MethodCFIInfo
    {
        /// <summary>FDE PC-begin (start of the (sub)function containing ip).</summary>
        public nuint MethodStart;
        /// <summary>FDE PC-end (= <see cref="MethodStart"/> + range).</summary>
        public nuint MethodEnd;
        /// <summary>Pointer to the augmentation block's LSDA (null if the FDE has no LSDA).</summary>
        public byte* pLSDA;
        /// <summary>Pointer to the FDE's CIE record (32-bit length field at offset 0).</summary>
        public byte* pCIE;
        /// <summary>First byte of FDE call-frame instructions (after the augmentation block).</summary>
        public byte* pFDEInstrs;
        /// <summary>One past the last FDE call-frame instruction byte.</summary>
        public byte* pFDEInstrsEnd;
    }

    /// <summary>Resolve <paramref name="ip"/> to its method's GCInfo. Returns false if no managed FDE/LSDA covers it.</summary>
    public static bool TryGetMethodGcInfo(nuint ip, out MethodGcInfo info)
    {
        info = default;

        if (!TryGetMethodCFI(ip, out MethodCFIInfo cfi) || cfi.pLSDA == null)
        {
            return false;
        }

        byte* p = cfi.pLSDA;
        byte unwindBlockFlags = *p++;

        byte* pMainLsda;
        nuint methodStartAddress;
        if ((unwindBlockFlags & UBF_FUNC_KIND_MASK) != UBF_FUNC_KIND_ROOT)
        {
            // Funclet: refers to the main function's blob, with its own start-delta.
            pMainLsda = p + *(int*)p;
            p += sizeof(int);
            methodStartAddress = cfi.MethodStart - (nuint)(nint)(*(int*)p);
            info.IsFunclet = true;
            info.IsFilter = (unwindBlockFlags & UBF_FUNC_KIND_MASK) == UBF_FUNC_KIND_FILTER;
        }
        else
        {
            pMainLsda = cfi.pLSDA;
            methodStartAddress = cfi.MethodStart;
        }

        // GetCodeOffset: skip optional associated-data / EH-info pointers at the main LSDA, then the GCInfo blob follows.
        byte* mp = pMainLsda;
        byte mainFlags = *mp++;
        if ((mainFlags & UBF_FUNC_HAS_ASSOCIATED_DATA) != 0)
        {
            mp += sizeof(int);
        }
        if ((mainFlags & UBF_FUNC_HAS_EHINFO) != 0)
        {
            mp += sizeof(int);
        }

        info.GcInfo = mp;
        info.CodeOffset = (uint)(ip - methodStartAddress);
        info.MethodStart = cfi.MethodStart;
        info.MethodEnd = cfi.MethodEnd;
        return true;
    }

    /// <summary>
    /// Resolve <paramref name="ip"/> to its method's FDE PC-begin and LSDA pointer. Mirrors the
    /// behaviour the exception handler relies on (<c>ExceptionHelper.TryGetMethodLSDA</c> delegates here).
    /// </summary>
    public static bool TryGetMethodLSDA(nuint ip, out nuint methodStart, out byte* pLSDA)
    {
        methodStart = 0;
        pLSDA = null;
        if (!TryGetMethodCFI(ip, out MethodCFIInfo cfi) || cfi.pLSDA == null)
        {
            return false;
        }
        methodStart = cfi.MethodStart;
        pLSDA = cfi.pLSDA;
        return true;
    }

    /// <summary>
    /// Walk <c>.eh_frame</c> and locate the FDE that covers <paramref name="ip"/>. Reports the
    /// FDE's PC range, LSDA pointer (may be null), CIE pointer, and FDE-instruction range. This is
    /// the single <c>.eh_frame</c> walker in the kernel; the exception dispatcher's CFI unwinder
    /// (<c>ExceptionHelper.UnwindOneFrameWithCFI</c>) and the precise GC stack scan both delegate here.
    /// </summary>
    internal static bool TryGetMethodCFI(nuint ip, out MethodCFIInfo info)
    {
        info = default;

        byte* ehFrameStart = EhFrameNative.GetStart();
        byte* ehFrameEnd = EhFrameNative.GetEnd();
        if (ehFrameStart == null || ehFrameEnd == null || ehFrameStart >= ehFrameEnd)
        {
            return false;
        }

        byte* p = ehFrameStart;
        while (p < ehFrameEnd)
        {
            uint length = *(uint*)p;
            if (length == 0 || length == 0xFFFFFFFF)
            {
                break; // end of section / extended length not supported
            }

            byte* recordStart = p;
            byte* recordEnd = p + 4 + length;
            p += 4;

            uint ciePointer = *(uint*)p;
            p += 4;
            if (ciePointer == 0)
            {
                p = recordEnd; // a CIE — skip
                continue;
            }

            // FDE: PC-begin (sdata4, pc-relative), then PC-range (4 bytes).
            int pcBeginRel = *(int*)p;
            nuint pcBegin = (nuint)(p + pcBeginRel);
            p += 4;
            uint pcRange = *(uint*)p;
            p += 4;
            nuint pcEnd = pcBegin + pcRange;

            if (ip >= pcBegin && ip < pcEnd)
            {
                info.MethodStart = pcBegin;
                info.MethodEnd = pcEnd;
                // CIE pointer is the offset from the position immediately after the length field
                // back to the referenced CIE record (per DWARF .eh_frame encoding).
                info.pCIE = (recordStart + 4) - ciePointer;

                uint augLen = ReadULEB128(ref p);
                if (augLen > 0)
                {
                    int lsdaRel = *(int*)p;
                    if (lsdaRel != 0)
                    {
                        info.pLSDA = p + lsdaRel;
                    }
                    p += (int)augLen;
                }

                info.pFDEInstrs = p;
                info.pFDEInstrsEnd = recordEnd;
                return true;
            }
            p = recordEnd;
        }
        return false;
    }

    /// <summary>
    /// Read a DWARF unsigned LEB128 integer at <paramref name="p"/>, advancing the pointer.
    /// Canonical copy for the whole kernel (<c>ExceptionHelper</c>'s CFI parsers use this too).
    /// </summary>
    internal static uint ReadULEB128(ref byte* p)
    {
        uint result = 0;
        int shift = 0;
        byte b;
        do
        {
            b = *p++;
            result |= (uint)(b & 0x7F) << shift;
            shift += 7;
        }
        while ((b & 0x80) != 0);
        return result;
    }
}
