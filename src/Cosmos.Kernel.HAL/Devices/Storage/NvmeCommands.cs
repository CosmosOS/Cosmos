// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>NVMe admin opcodes (NVM Express 1.4 §5.x).</summary>
internal static class NvmeAdminOp
{
    public const byte DeleteIoSq = 0x00;
    public const byte CreateIoSq = 0x01;
    public const byte DeleteIoCq = 0x04;
    public const byte CreateIoCq = 0x05;
    public const byte Identify = 0x06;
    public const byte SetFeatures = 0x09;
    public const byte GetFeatures = 0x0A;
}

/// <summary>NVMe NVM-command-set IO opcodes (NVM Express 1.4 §6.x).</summary>
internal static class NvmeIoOp
{
    public const byte Flush = 0x00;
    public const byte Write = 0x01;
    public const byte Read = 0x02;
}

/// <summary>Identify CNS values (NVM Express 1.4 §5.15).</summary>
internal static class NvmeCns
{
    public const byte Namespace = 0x00;
    public const byte Controller = 0x01;
    public const byte ActiveNamespaceList = 0x02;
}

/// <summary>
/// NVMe Submission Queue Entry (64 bytes). Backed by raw memory; the
/// helpers fill the spec-defined CDWs without using managed structs (so
/// the layout stays exactly as the controller expects regardless of
/// runtime padding).
/// </summary>
internal unsafe struct NvmeSqe
{
    /// <summary>Submission Queue Entry size in 64-bit words (64 bytes / 8).</summary>
    private const int SqeSizeQwords = 8;

    /// <summary>Byte offset of CDW0 (opcode/FUSE/PSDT/CID) within the SQE.</summary>
    private const ulong Cdw0Offset = 0x00;
    /// <summary>Byte offset of the Namespace Identifier (NSID) within the SQE.</summary>
    private const ulong NsidOffset = 0x04;
    /// <summary>Byte offset of PRP Entry 1 within the SQE.</summary>
    private const ulong Prp1Offset = 0x18;
    /// <summary>Byte offset of PRP Entry 2 within the SQE.</summary>
    private const ulong Prp2Offset = 0x20;
    /// <summary>Byte offset of CDW10 within the SQE.</summary>
    private const ulong Cdw10Offset = 0x28;
    /// <summary>Byte offset of CDW11 within the SQE.</summary>
    private const ulong Cdw11Offset = 0x2C;
    /// <summary>Byte offset of CDW12 within the SQE.</summary>
    private const ulong Cdw12Offset = 0x30;

    /// <summary>Bit position of the Command Identifier (CID) within CDW0.</summary>
    private const int CidShift = 16;

    public ulong Address;

    public NvmeSqe(ulong address)
    {
        Address = address;
        Clear();
    }

    public void Clear()
    {
        ulong* p = (ulong*)Address;
        for (int i = 0; i < SqeSizeQwords; i++)
        {
            p[i] = 0;
        }
    }

    public void SetOpcode(byte opcode, ushort cid)
    {
        // CDW0: opcode | FUSE | reserved | PSDT | CID<<16
        uint cdw0 = (uint)opcode | ((uint)cid << CidShift);
        Native.MMIO.Write32(Address + Cdw0Offset, cdw0);
    }

    public void SetNsid(uint nsid)
    {
        Native.MMIO.Write32(Address + NsidOffset, nsid);
    }

    public void SetPrp1(ulong physAddr)
    {
        Native.MMIO.Write64(Address + Prp1Offset, physAddr);
    }

    public void SetPrp2(ulong physAddr)
    {
        Native.MMIO.Write64(Address + Prp2Offset, physAddr);
    }

    public void SetCdw10(uint value)
    {
        Native.MMIO.Write32(Address + Cdw10Offset, value);
    }

    public void SetCdw11(uint value)
    {
        Native.MMIO.Write32(Address + Cdw11Offset, value);
    }

    public void SetCdw12(uint value)
    {
        Native.MMIO.Write32(Address + Cdw12Offset, value);
    }
}

/// <summary>
/// NVMe Completion Queue Entry (16 bytes). Reads only — the controller
/// writes these.
/// </summary>
internal unsafe struct NvmeCqe
{
    /// <summary>Byte offset of DW0 (command-specific result) within the CQE.</summary>
    private const ulong CommandSpecificOffset = 0x00;
    /// <summary>Byte offset of DW1 (reserved) within the CQE.</summary>
    private const ulong ReservedOffset = 0x04;
    /// <summary>Byte offset of the SQ Head Pointer within the CQE.</summary>
    private const ulong SqHeadPointerOffset = 0x08;
    /// <summary>Byte offset of the SQ Identifier within the CQE.</summary>
    private const ulong SqIdentifierOffset = 0x0A;
    /// <summary>Byte offset of the Command Identifier within the CQE.</summary>
    private const ulong CommandIdentifierOffset = 0x0C;
    /// <summary>Byte offset of the Status Field (including phase tag) within the CQE.</summary>
    private const ulong StatusFieldOffset = 0x0E;

    /// <summary>Mask selecting the phase tag bit in the status field.</summary>
    private const int PhaseTagMask = 0x1;
    /// <summary>Bit position of the status code within the status field (bit 0 is the phase tag).</summary>
    private const int StatusCodeShift = 1;
    /// <summary>Mask selecting the 15-bit status code after shifting out the phase tag.</summary>
    private const int StatusCodeMask = 0x7FFF;

    public ulong Address;

    public NvmeCqe(ulong address)
    {
        Address = address;
    }

    public uint CommandSpecific => Native.MMIO.Read32(Address + CommandSpecificOffset);
    public uint Reserved => Native.MMIO.Read32(Address + ReservedOffset);
    public ushort SqHeadPointer => Native.MMIO.Read16(Address + SqHeadPointerOffset);
    public ushort SqIdentifier => Native.MMIO.Read16(Address + SqIdentifierOffset);
    public ushort CommandIdentifier => Native.MMIO.Read16(Address + CommandIdentifierOffset);
    public ushort StatusField => Native.MMIO.Read16(Address + StatusFieldOffset);

    /// <summary>Phase tag bit (toggles each pass through the queue).</summary>
    public bool Phase => (StatusField & PhaseTagMask) != 0;

    /// <summary>Status code field — 0 means success.</summary>
    public uint StatusCode => (uint)((StatusField >> StatusCodeShift) & StatusCodeMask);
}
