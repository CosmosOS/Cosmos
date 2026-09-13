// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// NVMe controller register block accessor. Wraps the BAR0 virtual base
/// (BAR0 phys + HHDM offset) and exposes the spec registers via
/// <see cref="Native.MMIO"/>.
///
/// Layout follows NVM Express 1.4 §3.1.
/// </summary>
internal class NvmeRegisters
{
    /// <summary>CAP — Controller Capabilities register offset (RO, 64-bit, NVMe 1.4 §3.1.1).</summary>
    private const ulong CapOffset = 0x00;

    /// <summary>VS — Version register offset (RO, 32-bit, NVMe 1.4 §3.1.2). Public so callers probing the register block (e.g. MMIO-remap tests) share the spec offset.</summary>
    public const ulong VsOffset = 0x08;

    /// <summary>INTMS — Interrupt Mask Set register offset (RW, 32-bit, NVMe 1.4 §3.1.3).</summary>
    private const ulong IntmsOffset = 0x0C;

    /// <summary>INTMC — Interrupt Mask Clear register offset (RW, 32-bit, NVMe 1.4 §3.1.4).</summary>
    private const ulong IntmcOffset = 0x10;

    /// <summary>CC — Controller Configuration register offset (RW, 32-bit, NVMe 1.4 §3.1.5).</summary>
    private const ulong CcOffset = 0x14;

    /// <summary>CSTS — Controller Status register offset (RO, 32-bit, NVMe 1.4 §3.1.6).</summary>
    private const ulong CstsOffset = 0x1C;

    /// <summary>AQA — Admin Queue Attributes register offset (RW, 32-bit, NVMe 1.4 §3.1.9).</summary>
    private const ulong AqaOffset = 0x24;

    /// <summary>ASQ — Admin Submission Queue Base register offset (RW, 64-bit, NVMe 1.4 §3.1.10).</summary>
    private const ulong AsqOffset = 0x28;

    /// <summary>ACQ — Admin Completion Queue Base register offset (RW, 64-bit, NVMe 1.4 §3.1.11).</summary>
    private const ulong AcqOffset = 0x30;

    /// <summary>Mask for the CAP.MQES field (bits 15:0) — Maximum Queue Entries Supported.</summary>
    private const ulong CapMqesMask = 0xFFFF;

    /// <summary>Bit position of the CAP.TO field (bits 31:24) — Timeout.</summary>
    private const int CapToShift = 24;

    /// <summary>Mask for the CAP.TO field after shifting (8 bits).</summary>
    private const ulong CapToMask = 0xFF;

    /// <summary>Bit position of the CAP.DSTRD field (bits 35:32) — Doorbell Stride.</summary>
    private const int CapDstrdShift = 32;

    /// <summary>Mask for the CAP.DSTRD field after shifting (4 bits).</summary>
    private const ulong CapDstrdMask = 0xF;

    /// <summary>Byte offset of the first doorbell register from BAR0 (NVMe 1.4 §3.1.16: doorbells start at 0x1000).</summary>
    private const ulong DoorbellBaseOffset = 0x1000UL;

    /// <summary>Doorbell stride unit in bytes; actual stride is this value &lt;&lt; CAP.DSTRD.</summary>
    private const ulong DoorbellStrideUnitBytes = 4UL;

    /// <summary>Doorbell registers per queue id (one submission tail, one completion head).</summary>
    private const uint DoorbellsPerQueue = 2;

    private readonly ulong _base;

    // CAP.DSTRD is immutable hardware capability state: snapshot the
    // doorbell stride once instead of a 64-bit MMIO read of CAP on every
    // doorbell ring (two extra device register reads per I/O otherwise).
    private readonly ulong _doorbellStride;

    public NvmeRegisters(ulong baseVirtAddress)
    {
        _base = baseVirtAddress;
        _doorbellStride = DoorbellStrideUnitBytes << (int)DSTRD;
    }

    /// <summary>BAR0 virtual address (used by the doorbell helper).</summary>
    public ulong BaseAddress => _base;

    /// <summary>CAP — Controller Capabilities (RO, 64-bit, offset 0x00).</summary>
    public ulong CAP => Native.MMIO.Read64(_base + CapOffset);

    /// <summary>VS — Version (RO, 32-bit, offset 0x08).</summary>
    public uint VS => Native.MMIO.Read32(_base + VsOffset);

    /// <summary>INTMS — Interrupt Mask Set (RW, 32-bit, offset 0x0C).</summary>
    public uint INTMS
    {
        get => Native.MMIO.Read32(_base + IntmsOffset);
        set => Native.MMIO.Write32(_base + IntmsOffset, value);
    }

    /// <summary>INTMC — Interrupt Mask Clear (RW, 32-bit, offset 0x10).</summary>
    public uint INTMC
    {
        get => Native.MMIO.Read32(_base + IntmcOffset);
        set => Native.MMIO.Write32(_base + IntmcOffset, value);
    }

    /// <summary>CC — Controller Configuration (RW, 32-bit, offset 0x14).</summary>
    public uint CC
    {
        get => Native.MMIO.Read32(_base + CcOffset);
        set => Native.MMIO.Write32(_base + CcOffset, value);
    }

    /// <summary>CSTS — Controller Status (RO, 32-bit, offset 0x1C).</summary>
    public uint CSTS => Native.MMIO.Read32(_base + CstsOffset);

    /// <summary>AQA — Admin Queue Attributes (RW, 32-bit, offset 0x24).</summary>
    public uint AQA
    {
        get => Native.MMIO.Read32(_base + AqaOffset);
        set => Native.MMIO.Write32(_base + AqaOffset, value);
    }

    /// <summary>ASQ — Admin Submission Queue Base (RW, 64-bit, offset 0x28).</summary>
    public ulong ASQ
    {
        get => Native.MMIO.Read64(_base + AsqOffset);
        set => Native.MMIO.Write64(_base + AsqOffset, value);
    }

    /// <summary>ACQ — Admin Completion Queue Base (RW, 64-bit, offset 0x30).</summary>
    public ulong ACQ
    {
        get => Native.MMIO.Read64(_base + AcqOffset);
        set => Native.MMIO.Write64(_base + AcqOffset, value);
    }

    /// <summary>CAP.MQES — Maximum Queue Entries Supported (1-based).</summary>
    public uint MQES => (uint)(CAP & CapMqesMask) + 1;

    /// <summary>CAP.DSTRD — Doorbell Stride (in dwords, log2). Stride bytes = 4 &lt;&lt; DSTRD.</summary>
    public uint DSTRD => (uint)((CAP >> CapDstrdShift) & CapDstrdMask);

    /// <summary>CAP.TO — worst-case CSTS.RDY transition time, in 500 ms units.</summary>
    public uint TO => (uint)((CAP >> CapToShift) & CapToMask);

    /// <summary>
    /// Compute the byte address of submission-queue tail doorbell for
    /// queue id <paramref name="qid"/>. Doorbells live at BAR0 + 0x1000 +
    /// (2 * qid + 0) * (4 &lt;&lt; CAP.DSTRD).
    /// </summary>
    public ulong SubmissionDoorbell(uint qid)
    {
        return _base + DoorbellBaseOffset + (ulong)(DoorbellsPerQueue * qid) * _doorbellStride;
    }

    /// <summary>
    /// Compute the byte address of completion-queue head doorbell for
    /// queue id <paramref name="qid"/>.
    /// </summary>
    public ulong CompletionDoorbell(uint qid)
    {
        return _base + DoorbellBaseOffset + (ulong)(DoorbellsPerQueue * qid + 1) * _doorbellStride;
    }
}
