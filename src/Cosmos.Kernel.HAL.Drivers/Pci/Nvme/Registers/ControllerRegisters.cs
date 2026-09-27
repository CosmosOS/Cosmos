// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme.Registers;

/// <summary>
/// The controller registers this driver uses, at the start of the mapped
/// BAR 0, and the queues' doorbells behind them (NVM Express 1.4 s3.1).
/// </summary>
internal sealed class ControllerRegisters
{
    /// <summary>CAP: Controller Capabilities (RO, 64-bit).</summary>
    private const ulong CapabilitiesOffset = 0x00;

    /// <summary>INTMS: Interrupt Mask Set (RW1S, 32-bit).</summary>
    private const ulong InterruptMaskSetOffset = 0x0C;

    /// <summary>CC: Controller Configuration (RW, 32-bit).</summary>
    private const ulong ConfigurationOffset = 0x14;

    /// <summary>CSTS: Controller Status (RO, 32-bit).</summary>
    private const ulong StatusOffset = 0x1C;

    /// <summary>AQA: Admin Queue Attributes (RW, 32-bit).</summary>
    private const ulong AdminQueueAttributesOffset = 0x24;

    /// <summary>ASQ: Admin Submission Queue Base Address (RW, 64-bit).</summary>
    private const ulong AdminSubmissionQueueOffset = 0x28;

    /// <summary>ACQ: Admin Completion Queue Base Address (RW, 64-bit).</summary>
    private const ulong AdminCompletionQueueOffset = 0x30;

    /// <summary>CAP.MQES: Maximum Queue Entries Supported, 0-based (bits 15:0).</summary>
    private const ulong CapMqesMask = 0xFFFF;

    /// <summary>Bit position of CAP.TO, the ready timeout (bits 31:24).</summary>
    private const int CapToShift = 24;

    /// <summary>CAP.TO once shifted down (8 bits).</summary>
    private const ulong CapToMask = 0xFF;

    /// <summary>Bit position of CAP.DSTRD, the doorbell stride (bits 35:32).</summary>
    private const int CapDstrdShift = 32;

    /// <summary>CAP.DSTRD once shifted down (4 bits).</summary>
    private const ulong CapDstrdMask = 0xF;

    /// <summary>INTMS value masking every pin-based and MSI vector.</summary>
    private const uint AllVectors = 0xFFFFFFFF;

    /// <summary>Offset of the first doorbell register (s3.1.16).</summary>
    private const ulong DoorbellBaseOffset = 0x1000;

    /// <summary>Doorbell stride unit in bytes; the stride is this shifted left by CAP.DSTRD.</summary>
    private const ulong DoorbellStrideUnitBytes = 4;

    /// <summary>Doorbells per queue ID: the submission queue's tail, then the completion queue's head.</summary>
    private const uint DoorbellsPerQueue = 2;

    /// <summary>Bytes of one doorbell register.</summary>
    private const ulong DoorbellBytes = sizeof(uint);

    private readonly MmioRegion _registers;

    // CAP.DSTRD is immutable hardware capability state: snapshot the
    // doorbell stride once instead of a 64-bit MMIO read of CAP on every
    // doorbell ring (two extra device register reads per I/O otherwise).
    private readonly ulong _doorbellStride;

    /// <summary>CAP: Controller Capabilities.</summary>
    internal ulong Capabilities => _registers.Read64(CapabilitiesOffset);

    /// <summary>CAP.MQES + 1: the most entries a queue may have.</summary>
    internal uint MaximumQueueEntries => (uint)(Capabilities & CapMqesMask) + 1;

    /// <summary>CAP.TO: the worst-case time CSTS.RDY takes to follow CC.EN, in 500 ms units.</summary>
    internal uint ReadyTimeoutUnits => (uint)((Capabilities >> CapToShift) & CapToMask);

    /// <summary>CC: Controller Configuration.</summary>
    internal uint Configuration
    {
        get => _registers.Read32(ConfigurationOffset);
        set => _registers.Write32(ConfigurationOffset, value);
    }

    /// <summary>CSTS: Controller Status.</summary>
    internal uint Status => _registers.Read32(StatusOffset);

    /// <summary>Views the registers of a controller whose BAR 0 is <paramref name="registers"/>.</summary>
    internal ControllerRegisters(MmioRegion registers)
    {
        _registers = registers;
        _doorbellStride = DoorbellStrideUnitBytes << (int)((Capabilities >> CapDstrdShift) & CapDstrdMask);
    }

    /// <summary>
    /// True when BAR 0 reaches the doorbells of every queue up to
    /// <paramref name="lastQueueId"/>: a smaller BAR would make a doorbell
    /// write throw, in the interrupt handler among other places.
    /// </summary>
    internal bool CoversDoorbellsOf(uint lastQueueId) =>
        CompletionDoorbellOffset(lastQueueId) + DoorbellBytes <= _registers.Length;

    /// <summary>
    /// Programs the admin queue pair: AQA with both queues' 0-based sizes,
    /// then ASQ and ACQ with their device addresses. The controller must be
    /// disabled.
    /// </summary>
    internal void SetAdminQueues(uint attributes, ulong submissionQueue, ulong completionQueue)
    {
        _registers.Write32(AdminQueueAttributesOffset, attributes);
        _registers.Write64(AdminSubmissionQueueOffset, submissionQueue);
        _registers.Write64(AdminCompletionQueueOffset, completionQueue);
    }

    /// <summary>
    /// Masks every pin-based and MSI vector through INTMS. Only while MSI-X
    /// is off: the host must not touch INTMS once the controller is
    /// configured for MSI-X, whose own table masks its vectors (s3.1.3).
    /// </summary>
    internal void MaskAllVectors() => _registers.Write32(InterruptMaskSetOffset, AllVectors);

    /// <summary>
    /// Writes <paramref name="tail"/> to the submission queue tail doorbell
    /// of queue <paramref name="queueId"/>. The region orders every earlier
    /// store to DMA memory, the entries it hands over, before the write.
    /// </summary>
    internal void RingSubmissionDoorbell(uint queueId, uint tail) =>
        _registers.Write32(SubmissionDoorbellOffset(queueId), tail);

    /// <summary>Writes <paramref name="head"/> to the completion queue head doorbell of queue <paramref name="queueId"/>. Any context.</summary>
    internal void RingCompletionDoorbell(uint queueId, uint head) =>
        _registers.Write32(CompletionDoorbellOffset(queueId), head);

    /// <summary>BAR 0 + 0x1000 + (2 * queue ID) * (4 &lt;&lt; CAP.DSTRD).</summary>
    private ulong SubmissionDoorbellOffset(uint queueId) =>
        DoorbellBaseOffset + DoorbellsPerQueue * queueId * _doorbellStride;

    /// <summary>BAR 0 + 0x1000 + (2 * queue ID + 1) * (4 &lt;&lt; CAP.DSTRD).</summary>
    private ulong CompletionDoorbellOffset(uint queueId) =>
        DoorbellBaseOffset + (DoorbellsPerQueue * queueId + 1) * _doorbellStride;
}
