// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Nvme;

/// <summary>
/// The NVMe register offsets, field layouts, opcodes, queue parameters and
/// Identify data offsets (NVM Express 1.4) for <see cref="NvmeDriver"/>,
/// <see cref="NvmeState"/> and <see cref="NvmeNamespace"/>. The two on-wire
/// queue entries follow in this file, laid out sequentially with no padding
/// so a whole-struct write into DMA memory is the wire format; every field
/// is little-endian on the architectures the kernel runs on. Constants
/// only; any context.
/// </summary>
internal static class NvmeProtocol
{
    // --- Registers (section 3.1) ---

    /// <summary>CAP, 64-bit: MQES in bits 15:0, TO in bits 31:24, DSTRD in bits 35:32.</summary>
    public const ulong Cap = 0x00;

    /// <summary>VS, 32-bit: the controller's specification version.</summary>
    public const ulong Vs = 0x08;

    /// <summary>INTMS, 32-bit: interrupt mask set, for the pin and MSI vectors.</summary>
    public const ulong Intms = 0x0C;

    /// <summary>INTMC, 32-bit: interrupt mask clear.</summary>
    public const ulong Intmc = 0x10;

    /// <summary>CC, 32-bit: controller configuration.</summary>
    public const ulong Cc = 0x14;

    /// <summary>CSTS, 32-bit: controller status.</summary>
    public const ulong Csts = 0x1C;

    /// <summary>AQA, 32-bit: admin queue attributes, both sizes zero based.</summary>
    public const ulong Aqa = 0x24;

    /// <summary>ASQ, 64-bit: admin submission queue base.</summary>
    public const ulong Asq = 0x28;

    /// <summary>ACQ, 64-bit: admin completion queue base.</summary>
    public const ulong Acq = 0x30;

    /// <summary>The first doorbell's offset; the doorbells follow at the stride CAP.DSTRD gives.</summary>
    public const ulong DoorbellBase = 0x1000;

    /// <summary>The doorbell stride unit: the stride is this many bytes shifted left by CAP.DSTRD.</summary>
    public const ulong DoorbellStrideUnit = 4;

    /// <summary>Doorbells per queue id: the submission tail and the completion head.</summary>
    public const ulong DoorbellsPerQueue = 2;

    /// <summary>Mask of CAP.MQES, the largest queue size minus one.</summary>
    public const ulong CapMqesMask = 0xFFFF;

    /// <summary>Shift of CAP.TO, the ready transition budget in 500 ms units.</summary>
    public const int CapToShift = 24;

    /// <summary>Mask of CAP.TO after the shift.</summary>
    public const ulong CapToMask = 0xFF;

    /// <summary>Shift of CAP.DSTRD, the doorbell stride exponent.</summary>
    public const int CapDstrdShift = 32;

    /// <summary>Mask of CAP.DSTRD after the shift.</summary>
    public const ulong CapDstrdMask = 0xF;

    /// <summary>Milliseconds in one CAP.TO unit.</summary>
    public const uint TimeoutUnitMilliseconds = 500;

    /// <summary>CC.EN, bit 0.</summary>
    public const uint CcEnable = 1;

    /// <summary>The CC value that enables the controller: IOSQES 6 (64-byte entries), IOCQES 4 (16-byte entries), EN.</summary>
    public const uint CcEnableValue = (6 << 16) | (4 << 20) | CcEnable;

    /// <summary>CSTS.RDY, bit 0.</summary>
    public const uint CstsReady = 1;

    /// <summary>The AQA value for an admin queue pair of <see cref="QueueDepth"/> entries: ACQS in bits 27:16, ASQS in bits 11:0.</summary>
    public const uint AqaValue = ((QueueDepth - 1) << 16) | (QueueDepth - 1);

    /// <summary>The INTMS value masking every pin and MSI vector; MSI-X is unaffected.</summary>
    public const uint IntmsMaskAll = 0xFFFFFFFF;

    // --- Queues ---

    /// <summary>Entries in each queue, admin and I/O.</summary>
    public const int QueueDepth = 8;

    /// <summary>The one I/O queue pair's id; the admin pair is 0.</summary>
    public const int IoQueueId = 1;

    /// <summary>Commands in flight at most: a queue of N entries holds N minus one, or the wrapped tail reads as empty.</summary>
    public const int IoSlots = QueueDepth - 1;

    /// <summary>Ids the active namespace list carries: one page of 32-bit ids.</summary>
    public const int MaxNamespaceIds = 1024;

    /// <summary>The smallest LBADS the specification allows: 512-byte blocks.</summary>
    public const int MinimumLbaShift = 9;

    // --- Admin opcodes (section 5) ---

    /// <summary>Delete I/O Submission Queue.</summary>
    public const byte AdminDeleteIoSq = 0x00;

    /// <summary>Create I/O Submission Queue.</summary>
    public const byte AdminCreateIoSq = 0x01;

    /// <summary>Delete I/O Completion Queue.</summary>
    public const byte AdminDeleteIoCq = 0x04;

    /// <summary>Create I/O Completion Queue.</summary>
    public const byte AdminCreateIoCq = 0x05;

    /// <summary>Identify.</summary>
    public const byte AdminIdentify = 0x06;

    // --- I/O opcodes (section 6) ---

    /// <summary>Flush.</summary>
    public const byte IoFlush = 0x00;

    /// <summary>Write.</summary>
    public const byte IoWrite = 0x01;

    /// <summary>Read.</summary>
    public const byte IoRead = 0x02;

    // --- Identify (section 5.15) ---

    /// <summary>CNS 0: the Identify Namespace data structure of the NSID given.</summary>
    public const uint CnsNamespace = 0x00;

    /// <summary>CNS 1: the Identify Controller data structure; NSID 0.</summary>
    public const uint CnsController = 0x01;

    /// <summary>CNS 2: the active namespace id list; NSID 0.</summary>
    public const uint CnsActiveNamespaceList = 0x02;

    /// <summary>Byte offset of NSZE in the Identify Namespace data: the namespace size in blocks, 64-bit.</summary>
    public const int IdentifyNamespaceSizeOffset = 0;

    /// <summary>Byte offset of FLBAS in the Identify Namespace data: the LBA format index in bits 3:0.</summary>
    public const int IdentifyNamespaceFlbasOffset = 26;

    /// <summary>Mask of the LBA format index within FLBAS.</summary>
    public const int FlbasFormatIndexMask = 0x0F;

    /// <summary>Byte offset of the LBA format table in the Identify Namespace data.</summary>
    public const int IdentifyNamespaceLbafOffset = 128;

    /// <summary>Bytes of one LBA format entry: MS in bytes 0 and 1, LBADS in byte 2.</summary>
    public const int LbafEntryBytes = 4;

    /// <summary>Byte offset of LBADS within an LBA format entry.</summary>
    public const int LbafLbadsOffset = 2;

    // --- Create I/O queue words (sections 5.3 and 5.4) ---

    /// <summary>CDW10 of both create commands for the I/O pair: the size minus one in bits 31:16, the queue id in bits 15:0.</summary>
    public const uint CreateIoQueueCdw10 = ((QueueDepth - 1) << 16) | IoQueueId;

    /// <summary>PC, bit 0 of CDW11: the queue is physically contiguous.</summary>
    public const uint CreateQueuePhysicallyContiguous = 1;

    /// <summary>IEN, bit 1 of CDW11 of Create I/O Completion Queue: the queue raises its vector.</summary>
    public const uint CreateCqInterruptsEnabled = 1 << 1;

    /// <summary>Shift of IV in CDW11 of Create I/O Completion Queue: the message vector.</summary>
    public const int CreateCqVectorShift = 16;

    /// <summary>CDW11 of Create I/O Submission Queue: the completion queue id in bits 31:16, PC.</summary>
    public const uint CreateIoSqCdw11 = (IoQueueId << 16) | CreateQueuePhysicallyContiguous;

    // --- Entry fields ---

    /// <summary>Shift of the command id within CDW0; the opcode sits in bits 7:0.</summary>
    public const int CommandIdShift = 16;

    /// <summary>The phase tag, bit 0 of a completion entry's status word.</summary>
    public const ushort PhaseTagMask = 1;

    /// <summary>Shift of the status field within the status word; 0 after the shift means success.</summary>
    public const int StatusFieldShift = 1;
}

/// <summary>
/// A submission queue entry (NVM Express 1.4 section 4.2): 64 bytes, the
/// hardware layout, so the fields stay public. A command is written whole,
/// from a fresh instance with the fields it needs set, so every other
/// field is zero: no metadata pointer, no PRP2, no fused operation.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NvmeSqe
{
    /// <summary>The opcode in bits 7:0 and the command id in bits 31:16.</summary>
    public uint Cdw0;

    /// <summary>The namespace id, or 0 for a command that names none.</summary>
    public uint Nsid;

    /// <summary>Reserved.</summary>
    public ulong Reserved;

    /// <summary>The metadata pointer; unused.</summary>
    public ulong Metadata;

    /// <summary>PRP entry 1: the physical address of the data page.</summary>
    public ulong Prp1;

    /// <summary>PRP entry 2; always 0, every transfer fits one page.</summary>
    public ulong Prp2;

    /// <summary>Command dword 10.</summary>
    public uint Cdw10;

    /// <summary>Command dword 11.</summary>
    public uint Cdw11;

    /// <summary>Command dword 12.</summary>
    public uint Cdw12;

    /// <summary>Command dword 13.</summary>
    public uint Cdw13;

    /// <summary>Command dword 14.</summary>
    public uint Cdw14;

    /// <summary>Command dword 15.</summary>
    public uint Cdw15;
}

/// <summary>
/// A completion queue entry (NVM Express 1.4 section 4.6): 16 bytes, the
/// hardware layout, so the fields stay public. The controller writes it;
/// the driver reads <see cref="Status"/> first, with a volatile read, since
/// its phase tag says whether the rest of the entry is valid.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct NvmeCqe
{
    /// <summary>Dword 0: the command-specific result.</summary>
    public uint Dw0;

    /// <summary>Dword 1: reserved.</summary>
    public uint Dw1;

    /// <summary>The submission queue head the controller has consumed to.</summary>
    public ushort SqHead;

    /// <summary>The id of the submission queue the command came from.</summary>
    public ushort SqId;

    /// <summary>The command id of the entry this completes.</summary>
    public ushort CommandId;

    /// <summary>Bit 0 the phase tag; bits 15:1 the status field: SC in bits 8:1, SCT in bits 11:9. The command succeeded when the field is 0.</summary>
    public ushort Status;
}
