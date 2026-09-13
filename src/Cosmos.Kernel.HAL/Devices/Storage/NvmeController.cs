// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.Pci;
using SchedMutex = Cosmos.Kernel.Core.Scheduler.Mutex;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>
/// One initialized NVMe controller. Owns its admin and a single IO
/// queue pair, and the namespaces it discovered. The admin queue is
/// polled (it only runs once at init); the I/O queue is interrupt-driven
/// via MSI-X so callers of <see cref="Read"/> / <see cref="Write"/> can
/// yield instead of burning a CPU on a spinloop, and up to
/// <see cref="IoQueueDepth"/>-1 commands can be in flight concurrently
/// because every slot owns its own 4 KiB DMA bounce buffer.
///
/// <para>If MSI-X is unavailable (e.g. ARM64 today, or a pathological
/// PCI device with no MSI-X capability) the controller falls back to
/// polled completion, but in that mode commands are serialized through
/// an internal mutex.</para>
///
/// Limitations:
/// <list type="bullet">
/// <item>One IO submission and one IO completion queue (qid=1), depth 8.
/// At most depth-1 commands are in flight (NVMe queue-full rule: a
/// depth-N SQ holds N-1 entries, or the wrapped tail is indistinguishable
/// from an empty queue).</item>
/// <item>Single-PRP transfers — every command moves at most one 4 KiB
/// page through the slot's bounce buffer, so PRP2 is always 0. Namespaces
/// whose LBA format exceeds one page (or carries metadata) are skipped at
/// discovery.</item>
/// </list>
/// </summary>
internal unsafe class NvmeController
{
    private const uint AdminQueueDepth = 8;
    private const uint IoQueueDepth = 8;
    private const uint IoQueueId = 1;

    /// <summary>Size of one Submission Queue Entry in bytes (NVMe 1.4 §4.2, fixed 64-byte SQE).</summary>
    private const int SqeSizeBytes = 64;
    /// <summary>Size of one Completion Queue Entry in bytes (NVMe 1.4 §4.6, fixed 16-byte CQE).</summary>
    private const int CqeSizeBytes = 16;
    /// <summary>Bytes per 64-bit word, used when zeroing a page one ulong at a time.</summary>
    private const int BytesPerUlong = 8;

    /// <summary>CC.EN — controller enable bit (bit 0 of the CC register).</summary>
    private const uint CcEnable = 1u;
    /// <summary>CC.IOSQES — log2 of the I/O SQ entry size (6 → 64-byte SQE).</summary>
    private const uint CcIoSqes = 6u;
    /// <summary>Bit position of the CC.IOSQES field (bits [19:16]).</summary>
    private const int CcIoSqesShift = 16;
    /// <summary>CC.IOCQES — log2 of the I/O CQ entry size (4 → 16-byte CQE).</summary>
    private const uint CcIoCqes = 4u;
    /// <summary>Bit position of the CC.IOCQES field (bits [23:20]).</summary>
    private const int CcIoCqesShift = 20;
    /// <summary>CSTS.RDY — controller ready bit (bit 0 of the CSTS register).</summary>
    private const uint CstsReady = 1u;
    /// <summary>INTMS value masking every pin-based interrupt vector (all 32 bits set).</summary>
    private const uint IntmsMaskAllVectors = 0xFFFFFFFF;
    /// <summary>Bit position of the AQA.ACQS field (admin CQ size, bits [27:16]).</summary>
    private const int AqaAcqsShift = 16;

    /// <summary>CAP.TO unit in milliseconds (NVMe 1.4 §3.1.1: TO counts 500 ms units).</summary>
    private const uint CapToUnitMs = 500;
    /// <summary>Microseconds per millisecond, for converting the 1 ms poll step to DelayMicroseconds units.</summary>
    private const uint MicrosecondsPerMillisecond = 1000;
    /// <summary>Spin/wait budget (iterations) before a command completion is declared timed out.</summary>
    private const int CommandTimeoutSpinCount = 50_000_000;

    /// <summary>Mask selecting the low 32 bits of the starting LBA for CDW10 (NVMe 1.4 §6.9, SLBA[31:0]).</summary>
    private const uint LbaLowDwordMask = 0xFFFFFFFF;
    /// <summary>Right-shift extracting the high 32 bits of the starting LBA for CDW11 (SLBA[63:32]).</summary>
    private const int LbaHighDwordShift = 32;
    /// <summary>Bit position of the queue-size-minus-one field in Create IO CQ/SQ CDW10 (bits [31:16]).</summary>
    private const int CreateQueueCdw10SizeShift = 16;
    /// <summary>Bit position of the interrupt vector field in Create IO CQ CDW11 (IV, bits [31:16]).</summary>
    private const int Cdw11IvShift = 16;
    /// <summary>IEN — interrupts enabled flag in Create IO CQ CDW11 (bit 1).</summary>
    private const uint Cdw11InterruptEnable = 1u << 1;
    /// <summary>PC — physically contiguous flag in Create IO CQ/SQ CDW11 (bit 0).</summary>
    private const uint Cdw11PhysicallyContiguous = 1u;
    /// <summary>Bit position of the completion queue ID field in Create IO SQ CDW11 (CQID, bits [31:16]).</summary>
    private const int Cdw11CqidShift = 16;
    /// <summary>Minimum MSI-X table size needed to park the I/O CQ on entry 1, away from admin IV 0.</summary>
    private const int MsiXMinEntriesForDedicatedIoVector = 2;

    /// <summary>Maximum NSIDs returned by Identify Active Namespace List (one 4 KiB page of 32-bit NSIDs).</summary>
    private const int MaxActiveNamespaceIds = 1024;
    /// <summary>Byte offset of FLBAS in the Identify Namespace data structure (NVMe 1.4 §5.15.2).</summary>
    private const int IdentifyNsFlbasOffset = 26;
    /// <summary>FLBAS bits [3:0] — index of the active LBA Format.</summary>
    private const int FlbasFormatIndexMask = 0x0F;
    /// <summary>Byte offset of the LBA Format table in the Identify Namespace data structure.</summary>
    private const int IdentifyNsLbafTableOffset = 128;
    /// <summary>Size of one LBA Format descriptor in bytes (MS in bytes 0-1, LBADS in byte 2).</summary>
    private const int LbafEntrySizeBytes = 4;
    /// <summary>Byte offset of the LBADS field within an LBA Format descriptor.</summary>
    private const int LbafLbadsOffset = 2;
    /// <summary>Minimum LBADS allowed by NVMe 1.4 (2^9 = 512-byte logical blocks).</summary>
    private const int MinSupportedLbads = 9;

    /// <summary>
    /// Per-CID bookkeeping for in-flight I/O commands. Each slot carries
    /// its own bounce buffer so multiple namespaces (or threads) can hold
    /// independent in-flight commands without trampling one shared page.
    /// </summary>
    private sealed class IoSlot
    {
        public InterruptEvent Done = new();
        public uint Status;
        public bool InUse;
        public ulong DmaBufferVirt;
        public ulong DmaBufferPhys;
    }

    private readonly PciDevice _pci;
    private readonly NvmeRegisters _regs;

    // Admin queue
    private ulong _adminSqVirt;
    private ulong _adminCqVirt;
    private ulong _adminSqPhys;
    private ulong _adminCqPhys;
    private uint _adminSqTail;
    private uint _adminCqHead;
    private bool _adminCqPhase;
    private ushort _adminCmdId;

    // IO queue
    private ulong _ioSqVirt;
    private ulong _ioCqVirt;
    private ulong _ioSqPhys;
    private ulong _ioCqPhys;
    private uint _ioSqTail;
    private uint _ioCqHead;
    private bool _ioCqPhase;

    // I/O completion plumbing (MSI-X driven when possible).
    private MsiXContext _msiX;
    private bool _msiXEnabled;
    private int _ioMsiXEntry;
    private IoSlot[]? _ioSlots;

    // Slot allocation. SpinLock guards both the slot in-use bits and the
    // _slotWaiters queue. SubmitSqLock guards the SQ tail + doorbell
    // critical section so concurrent submits don't clobber _ioSqTail.
    private SchedSpinLock _slotLock;
    private SchedSpinLock _submitSqLock;
    private readonly List<SchedulerThread> _slotWaiters = [];

    // Polled-completion fallback path: serializes the entire submit/wait
    // sequence so concurrent callers don't race on _ioCqHead / _ioCqPhase.
    // Unused once MSI-X is enabled.
    private readonly SchedMutex _polledIoMutex = new();

    public List<NvmeNamespace> Namespaces { get; } = new();

    /// <summary>
    /// True once I/O completions are delivered via MSI-X; false when the
    /// controller fell back to polled I/O (no MSI-X capability, or no platform
    /// MSI routing backend — e.g. an ARM64 GIC with no ITS). Lets a test assert
    /// which interrupt path a given QEMU profile actually exercised rather than
    /// just that I/O works.
    /// </summary>
    public bool IsMsiXEnabled => _msiXEnabled;

    /// <summary>
    /// Zero-based index of this controller in PCI discovery order. Used to
    /// build unique namespace device names ("nvme0n1" style).
    /// </summary>
    public int Index { get; }

    public NvmeController(PciDevice pci, int index)
    {
        Index = index;
        _pci = pci;
        pci.EnableBusMaster(true);
        pci.EnableMemory(true);

        if (pci.BaseAddressBar == null || pci.BaseAddressBar.Length < 1)
        {
            throw new Exception("[NVMe] Invalid BAR configuration");
        }

        ulong bar0Phys = pci.GetBar64Address(0);
        if (bar0Phys == 0)
        {
            throw new Exception("[NVMe] BAR0 is not a memory BAR");
        }

        // ARM64 needs the BAR page installed as Device memory in TTBR1
        // before the HHDM virtual is dereferenceable for MMIO. No-op on x64.
        PlatformHAL.Initializer?.EnsureMmioMapped(bar0Phys);

        ulong hhdmOffset = Limine.HHDM.Response != null ? Limine.HHDM.Response->Offset : 0;
        ulong bar0Virt = bar0Phys + hhdmOffset;
        _regs = new NvmeRegisters(bar0Virt);

        Serial.WriteString("[NVMe] BAR0 phys=0x");
        Serial.WriteHex(bar0Phys);
        Serial.WriteString(" virt=0x");
        Serial.WriteHex(bar0Virt);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// Bring the controller up: reset, set up admin queues, identify
    /// controller and namespaces, create one IO queue pair, register
    /// each namespace as a BlockDevice.
    /// </summary>
    public void Initialize()
    {
        // NVMe 1.4: a queue may not exceed CAP.MQES+1 entries. Our fixed
        // depth is tiny (8), but a controller reporting less would silently
        // get an out-of-spec queue size (Invalid Queue Size on create, or
        // worse) — fail init cleanly instead; Nvme.Initialize's per-
        // controller catch skips the device.
        if (_regs.MQES < IoQueueDepth)
        {
            throw new Exception("[NVMe] Controller MQES below the driver's queue depth");
        }

        DisableController();
        SetupAdminQueues();
        EnableController();

        // Mask every pin-based interrupt vector: admin completions (and all
        // I/O in the polled fallback) would otherwise assert the
        // level-triggered INTx line until the CQ head doorbell write — a
        // spurious-interrupt source on any platform where that GSI is
        // unmasked or shared. MSI-X, when it comes up later, is unaffected
        // by INTMS, and MsiX.Enable also sets PCI Command.InterruptDisable.
        _regs.INTMS = IntmsMaskAllVectors;

        Serial.WriteString("[NVMe] Controller ready\n");

        DiscoverNamespaces();
        SetupIoCompletionPlumbing();
        CreateIoQueues();
    }

    /// <summary>
    /// Allocate per-slot DMA buffers + InterruptEvent, enable MSI-X on
    /// the device, allocate an interrupt vector, and program the MSI-X
    /// table to deliver to it. Falls back to polled mode if the device
    /// has no MSI-X cap or the platform has no MSI routing backend.
    /// </summary>
    private void SetupIoCompletionPlumbing()
    {
        // Depth-1 slots: a depth-N submission queue holds at most N-1
        // outstanding entries (NVMe 1.4 §4.1) — with N in flight the
        // wrapped tail equals the head and the controller reads the queue
        // as empty, silently losing a command.
        _ioSlots = new IoSlot[IoQueueDepth - 1];
        for (int i = 0; i < _ioSlots.Length; i++)
        {
            IoSlot slot = new();
            slot.DmaBufferVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
            slot.DmaBufferPhys = PageAllocator.VirtualToPhysical(slot.DmaBufferVirt);
            _ioSlots[i] = slot;
        }

        MsiXContext? ctx = MsiX.Enable(_pci);
        if (ctx == null)
        {
            Serial.WriteString("[NVMe] MSI-X unavailable, falling back to polled I/O\n");
            return;
        }

        _msiX = ctx.Value;
        // The binder allocates the underlying vector / LPI itself (x64 IDT
        // vector or ARM64 LPI INTID) and wires it to OnIoCompletion. Use
        // entry 1 when the table has one: the admin CQ is hardwired to
        // IV 0, so parking the IO CQ there means every admin completion
        // after MSI-X enable fires OnIoCompletion spuriously. Entry 0
        // stays masked (admin commands are polled), so admin completions
        // interrupt nobody.
        _ioMsiXEntry = _msiX.EntryCount >= MsiXMinEntriesForDedicatedIoVector ? 1 : 0;
        MsiX.SetEntry(_msiX, _ioMsiXEntry, OnIoCompletion);
        _msiXEnabled = true;

        Serial.WriteString("[NVMe] I/O CQ -> MSI-X entry ");
        Serial.WriteNumber((uint)_ioMsiXEntry);
        Serial.WriteString("\n");
    }

    private void DisableController()
    {
        uint cc = _regs.CC;
        if ((cc & CcEnable) != 0)
        {
            _regs.CC = cc & ~CcEnable;
        }

        WaitForReady(expected: false, "[NVMe] Timeout waiting for CSTS.RDY=0");
    }

    private void EnableController()
    {
        // CC.IOSQES=6 (64-byte SQE), CC.IOCQES=4 (16-byte CQE), CC.MPS=0 (4K), CC.CSS=0, EN=1
        uint cc = (CcIoSqes << CcIoSqesShift) | (CcIoCqes << CcIoCqesShift) | CcEnable;
        _regs.CC = cc;

        WaitForReady(expected: true, "[NVMe] Timeout waiting for CSTS.RDY=1");
    }

    // NVMe 1.4 s3.1.1: software must wait up to CAP.TO (500 ms units, up
    // to 63.5 s) for CSTS.RDY to track CC.EN. The old fixed 1M-iteration
    // MMIO spin was ~0.3-2 s depending on read latency — healthy drives
    // with slow bring-up would spuriously fail init and be skipped.
    private void WaitForReady(bool expected, string timeoutMessage)
    {
        // At least one 500 ms unit even if a controller reports TO=0.
        uint budgetMs = (_regs.TO == 0 ? 1 : _regs.TO) * CapToUnitMs;
        for (uint elapsedMs = 0; ; elapsedMs++)
        {
            if (((_regs.CSTS & CstsReady) != 0) == expected)
            {
                return;
            }

            if (elapsedMs >= budgetMs)
            {
                throw new Exception(timeoutMessage);
            }

            PlatformHAL.Initializer?.DelayMicroseconds(MicrosecondsPerMillisecond);
        }
    }

    private void SetupAdminQueues()
    {
        _adminSqVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        _adminCqVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        _adminSqPhys = PageAllocator.VirtualToPhysical(_adminSqVirt);
        _adminCqPhys = PageAllocator.VirtualToPhysical(_adminCqVirt);
        _adminSqTail = 0;
        _adminCqHead = 0;
        _adminCqPhase = true;

        // AQA: ACQS in [27:16], ASQS in [11:0], both 0-based.
        _regs.AQA = ((AdminQueueDepth - 1) << AqaAcqsShift) | (AdminQueueDepth - 1);
        _regs.ASQ = _adminSqPhys;
        _regs.ACQ = _adminCqPhys;
    }

    /// <summary>
    /// Submit an admin command and poll for its completion.
    /// Returns the CQE status code (0 = success).
    /// </summary>
    private uint SubmitAdmin(byte opcode, uint nsid, ulong prp1, uint cdw10, uint cdw11, uint cdw12)
    {
        ushort cid = _adminCmdId++;

        NvmeSqe sqe = new(_adminSqVirt + (ulong)_adminSqTail * SqeSizeBytes);
        sqe.SetOpcode(opcode, cid);
        sqe.SetNsid(nsid);
        sqe.SetPrp1(prp1);
        sqe.SetPrp2(0);
        sqe.SetCdw10(cdw10);
        sqe.SetCdw11(cdw11);
        sqe.SetCdw12(cdw12);

        _adminSqTail = (_adminSqTail + 1) % AdminQueueDepth;
        // Order the SQE stores (Normal memory) before the doorbell store
        // (Device memory): ARM64 does not order the two on its own, so the
        // controller could otherwise fetch a half-written command.
        PlatformHAL.Initializer?.DmaBarrier();
        Native.MMIO.Write32(_regs.SubmissionDoorbell(0), _adminSqTail);

        return WaitCompletion(_adminCqVirt, ref _adminCqHead, ref _adminCqPhase, AdminQueueDepth, qid: 0, expectCid: cid);
    }

    /// <summary>
    /// Read <paramref name="dst"/>.Length bytes (must equal block size *
    /// (numLogicalBlocksMinusOne+1)) starting at LBA <paramref name="lba"/>
    /// from namespace <paramref name="nsid"/>. Thread-safe — multiple
    /// callers run on independent slots up to <see cref="IoQueueDepth"/>-1.
    /// </summary>
    public uint Read(uint nsid, ulong lba, Span<byte> dst, ushort numLogicalBlocksMinusOne)
    {
        ValidateTransfer(dst.Length, numLogicalBlocksMinusOne);
        int slotIndex = AcquireSlot();
        try
        {
            IoSlot slot = _ioSlots![slotIndex];
            uint sc = SubmitOnSlot(NvmeIoOp.Read, nsid, slotIndex, lba, numLogicalBlocksMinusOne);
            if (sc == 0)
            {
                CopyOut(slot.DmaBufferVirt, dst);
            }

            ReleaseSlot(slotIndex);
            return sc;
        }
        catch
        {
            QuarantineSlot(slotIndex);
            throw;
        }
    }

    /// <summary>
    /// Write <paramref name="src"/> to namespace <paramref name="nsid"/>
    /// starting at LBA <paramref name="lba"/>. Thread-safe.
    /// </summary>
    public uint Write(uint nsid, ulong lba, ReadOnlySpan<byte> src, ushort numLogicalBlocksMinusOne)
    {
        ValidateTransfer(src.Length, numLogicalBlocksMinusOne);
        int slotIndex = AcquireSlot();
        try
        {
            IoSlot slot = _ioSlots![slotIndex];
            CopyIn(src, slot.DmaBufferVirt);
            if ((ulong)src.Length < PageAllocator.PageSize)
            {
                // The controller can't know the namespace block size at this
                // layer, so a span shorter than the device transfer would
                // otherwise write the previous command's bounce residue to
                // disk — zero the tail so short writes are deterministic.
                MemoryOp.MemSet((byte*)(slot.DmaBufferVirt + (ulong)src.Length), 0, (int)(PageAllocator.PageSize - (ulong)src.Length));
            }
            uint sc = SubmitOnSlot(NvmeIoOp.Write, nsid, slotIndex, lba, numLogicalBlocksMinusOne);

            ReleaseSlot(slotIndex);
            return sc;
        }
        catch
        {
            QuarantineSlot(slotIndex);
            throw;
        }
    }

    /// <summary>Flush volatile write cache for namespace <paramref name="nsid"/>.</summary>
    public uint Flush(uint nsid)
    {
        int slotIndex = AcquireSlot();
        try
        {
            uint sc = SubmitOnSlot(NvmeIoOp.Flush, nsid, slotIndex, 0, 0);

            ReleaseSlot(slotIndex);
            return sc;
        }
        catch
        {
            QuarantineSlot(slotIndex);
            throw;
        }
    }

    /// <summary>
    /// The single-PRP data path moves at most one 4 KiB page per command;
    /// larger transfers would make the device chase PRP2 (always 0 here),
    /// i.e. DMA through physical address 0. The device-side length is
    /// (numLogicalBlocksMinusOne+1) * blockSize regardless of the span the
    /// caller hands over, so multi-block commands are rejected outright —
    /// the span alone can't prove the device transfer fits the page.
    /// </summary>
    private static void ValidateTransfer(int byteLength, ushort numLogicalBlocksMinusOne)
    {
        if (numLogicalBlocksMinusOne != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numLogicalBlocksMinusOne),
                "The single-PRP data path issues one logical block per command.");
        }

        if (byteLength <= 0 || (ulong)byteLength > PageAllocator.PageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength),
                "NVMe transfers are limited to one 4 KiB page (single-PRP data path).");
        }
    }

    /// <summary>
    /// Called when a command failed in a way that may leave it outstanding
    /// in the controller (e.g. polled timeout). Deliberately leaves the
    /// slot's InUse flag set so it is never handed out again: a straggling
    /// completion would otherwise be misattributed to a new command and
    /// DMA into a recycled buffer. The (rare) slot leak is the price of
    /// containment.
    /// </summary>
    private void QuarantineSlot(int index)
    {
        Serial.WriteString("[NVMe] Quarantined I/O slot ");
        Serial.WriteNumber((uint)index);
        Serial.WriteString(" (command may still be outstanding)\n");
    }

    /// <summary>
    /// Find a free slot, mark it in-use, and return its index. If every
    /// slot is in flight, blocks the caller on the slot waiter queue
    /// until <see cref="ReleaseSlot"/> wakes one. Without a scheduler
    /// thread context (scheduler feature off, or pre-scheduler boot code)
    /// there is a single execution context, so a free slot always exists
    /// and no waiting is ever needed.
    /// </summary>
    private int AcquireSlot()
    {
        SchedulerThread? current = SchedulerManager.IsReady
            ? SchedulerManager.GetCpuState(SchedulerManager.GetCurrentCpuId()).CurrentThread
            : null;
        if (current == null)
        {
            for (int i = 0; i < _ioSlots!.Length; i++)
            {
                if (!_ioSlots[i].InUse)
                {
                    _ioSlots[i].InUse = true;
                    return i;
                }
            }

            throw new InvalidOperationException("NVMe I/O slots exhausted without a scheduler context.");
        }

        while (true)
        {
            // IRQ-safe scope for the whole check-register-block sequence:
            // releasing the lock before BlockThread opens the classic
            // lost-wakeup window — a ReleaseSlot racing in between would
            // ready a still-Running thread and the subsequent BlockThread
            // would bury the wakeup forever.
            using (_slotLock.AcquireIrqSafe())
            {
                for (int i = 0; i < _ioSlots!.Length; i++)
                {
                    if (!_ioSlots[i].InUse)
                    {
                        _ioSlots[i].InUse = true;
                        return i;
                    }
                }

                if (!_slotWaiters.Contains(current))
                {
                    _slotWaiters.Add(current);
                }

                SchedulerManager.BlockThread(current.CpuId, current);
            }

            InternalCpu.Halt();
        }
    }

    private void ReleaseSlot(int index)
    {
        SchedulerThread? waiter = null;
        using (_slotLock.AcquireIrqSafe())
        {
            _ioSlots![index].InUse = false;
            if (_slotWaiters.Count > 0)
            {
                waiter = _slotWaiters[0];
                _slotWaiters.RemoveAt(0);
            }
        }

        if (waiter != null)
        {
            SchedulerManager.ReadyThread(waiter.CpuId, waiter);
        }
    }

    /// <summary>
    /// Build the SQE for an already-acquired slot, ring the SQ doorbell,
    /// and wait for the matching CQE. Returns the device's status code
    /// (0 = success). Thread-safe.
    /// </summary>
    private uint SubmitOnSlot(byte opcode, uint nsid, int slotIndex, ulong lba, ushort numLogicalBlocksMinusOne)
    {
        IoSlot slot = _ioSlots![slotIndex];
        slot.Status = 0;
        ushort cid = (ushort)slotIndex;

        if (_msiXEnabled)
        {
            // Submit under the SQ lock, then wait outside it so other
            // threads can enqueue while this one is parked.
            _submitSqLock.Acquire();
            try
            {
                NvmeSqe sqe = new(_ioSqVirt + (ulong)_ioSqTail * SqeSizeBytes);
                sqe.SetOpcode(opcode, cid);
                sqe.SetNsid(nsid);
                sqe.SetPrp1(slot.DmaBufferPhys);
                sqe.SetPrp2(0);
                sqe.SetCdw10((uint)(lba & LbaLowDwordMask));
                sqe.SetCdw11((uint)(lba >> LbaHighDwordShift));
                sqe.SetCdw12(numLogicalBlocksMinusOne);

                _ioSqTail = (_ioSqTail + 1) % IoQueueDepth;
                // SQE stores must be visible to the device before the
                // doorbell store (see SubmitAdmin).
                PlatformHAL.Initializer?.DmaBarrier();
                Native.MMIO.Write32(_regs.SubmissionDoorbell(IoQueueId), _ioSqTail);
            }
            finally
            {
                _submitSqLock.Release();
            }

            // Hang-breaker mirroring the polled fallback's 50M-spin budget:
            // a lost or misrouted MSI-X message must surface as the same
            // timeout exception (the caller's catch quarantines the slot)
            // instead of parking the thread forever with no diagnostic.
            if (!slot.Done.Wait(CommandTimeoutSpinCount))
            {
                throw new Exception("[NVMe] Timeout waiting for command completion");
            }

            return slot.Status;
        }

        // Polled fallback: the CQ head/phase aren't safe to share, so
        // serialize the whole submit/wait sequence.
        _polledIoMutex.Acquire();
        try
        {
            NvmeSqe sqe = new(_ioSqVirt + (ulong)_ioSqTail * SqeSizeBytes);
            sqe.SetOpcode(opcode, cid);
            sqe.SetNsid(nsid);
            sqe.SetPrp1(slot.DmaBufferPhys);
            sqe.SetPrp2(0);
            sqe.SetCdw10((uint)(lba & LbaLowDwordMask));
            sqe.SetCdw11((uint)(lba >> LbaHighDwordShift));
            sqe.SetCdw12(numLogicalBlocksMinusOne);

            _ioSqTail = (_ioSqTail + 1) % IoQueueDepth;
            // SQE stores must be visible to the device before the
            // doorbell store (see SubmitAdmin).
            PlatformHAL.Initializer?.DmaBarrier();
            Native.MMIO.Write32(_regs.SubmissionDoorbell(IoQueueId), _ioSqTail);

            slot.Status = WaitCompletion(_ioCqVirt, ref _ioCqHead, ref _ioCqPhase, IoQueueDepth, qid: IoQueueId, expectCid: cid);
            return slot.Status;
        }
        finally
        {
            _polledIoMutex.Release();
        }
    }

    /// <summary>
    /// MSI-X handler for the I/O completion queue. Drains every CQE
    /// whose phase matches the expected phase, advances the CQ head,
    /// rings the doorbell, and signals each slot's
    /// <see cref="InterruptEvent"/>. Performs no allocation and no
    /// interface dispatch (per the project's ISR-safety rules).
    /// </summary>
    private void OnIoCompletion(ref IRQContext context)
    {
        if (_ioSlots == null || _ioCqVirt == 0)
        {
            return;
        }

        bool drained = false;
        while (true)
        {
            NvmeCqe cqe = new(_ioCqVirt + (ulong)_ioCqHead * CqeSizeBytes);
            if (cqe.Phase != _ioCqPhase)
            {
                break;
            }

            // Read barrier: don't consume CID/status (or the DMA'd payload
            // they guard) ahead of the device-written phase bit.
            PlatformHAL.Initializer?.DmaBarrier();

            ushort cid = cqe.CommandIdentifier;
            uint sc = cqe.StatusCode;

            _ioCqHead = (_ioCqHead + 1) % IoQueueDepth;
            if (_ioCqHead == 0)
            {
                _ioCqPhase = !_ioCqPhase;
            }
            drained = true;

            if (cid < _ioSlots.Length)
            {
                IoSlot slot = _ioSlots[cid];
                slot.Status = sc;
                slot.Done.Signal();
            }
        }

        if (drained)
        {
            Native.MMIO.Write32(_regs.CompletionDoorbell(IoQueueId), _ioCqHead);
        }
    }

    private uint WaitCompletion(ulong cqBase, ref uint head, ref bool expectedPhase, uint depth, uint qid, ushort expectCid)
    {
        uint spin = 0;
        while (true)
        {
            NvmeCqe cqe = new(cqBase + (ulong)head * CqeSizeBytes);
            if (cqe.Phase == expectedPhase)
            {
                // Read barrier: the phase bit is device-written; the rest of
                // the CQE (and the DMA'd payload it guards) must not be read
                // ahead of it on weakly-ordered ARM64.
                PlatformHAL.Initializer?.DmaBarrier();

                uint sc = cqe.StatusCode;
                ushort cid = cqe.CommandIdentifier;

                head = (head + 1) % depth;
                if (head == 0)
                {
                    expectedPhase = !expectedPhase;
                }
                Native.MMIO.Write32(_regs.CompletionDoorbell(qid), head);

                if (cid != expectCid)
                {
                    // Stale CQE from an abandoned command (e.g. an earlier
                    // polled timeout): consume it and keep waiting for the
                    // expected CID instead of misattributing its status.
                    Serial.WriteString("[NVMe] Consumed stale CQE (cid ");
                    Serial.WriteNumber(cid);
                    Serial.WriteString(", expected ");
                    Serial.WriteNumber(expectCid);
                    Serial.WriteString(")\n");
                    continue;
                }

                return sc;
            }

            if (++spin > CommandTimeoutSpinCount)
            {
                throw new Exception("[NVMe] Timeout waiting for command completion");
            }
        }
    }

    private void DiscoverNamespaces()
    {
        ulong identifyVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        ulong identifyPhys = PageAllocator.VirtualToPhysical(identifyVirt);

        try
        {
            // Identify Controller (CNS=0x01) — only used to confirm the controller is responsive.
            uint sc = SubmitAdmin(NvmeAdminOp.Identify, nsid: 0, prp1: identifyPhys, cdw10: NvmeCns.Controller, cdw11: 0, cdw12: 0);
            if (sc != 0)
            {
                Serial.WriteString("[NVMe] Identify Controller failed, status=0x");
                Serial.WriteHex(sc);
                Serial.WriteString("\n");
                return;
            }

            // Identify Active Namespace List (CNS=0x02) — returns up to 1024 NSIDs.
            ZeroPage(identifyVirt);
            sc = SubmitAdmin(NvmeAdminOp.Identify, nsid: 0, prp1: identifyPhys, cdw10: NvmeCns.ActiveNamespaceList, cdw11: 0, cdw12: 0);
            if (sc != 0)
            {
                Serial.WriteString("[NVMe] Identify Active NS List failed, status=0x");
                Serial.WriteHex(sc);
                Serial.WriteString("\n");
                return;
            }

            // Snapshot the namespace list before reusing the buffer for
            // per-namespace identifies — RegisterNamespace overwrites the
            // page, so we cannot iterate nsidList in place.
            uint* nsidList = (uint*)identifyVirt;
            Span<uint> nsids = stackalloc uint[MaxActiveNamespaceIds];
            int nsCount = 0;
            for (int i = 0; i < MaxActiveNamespaceIds; i++)
            {
                uint nsid = nsidList[i];
                if (nsid == 0)
                {
                    break;
                }
                nsids[nsCount++] = nsid;
            }

            for (int i = 0; i < nsCount; i++)
            {
                RegisterNamespace(nsids[i], identifyVirt, identifyPhys);
            }
        }
        finally
        {
            PageAllocator.Free((void*)identifyVirt);
        }
    }

    private void RegisterNamespace(uint nsid, ulong identifyVirt, ulong identifyPhys)
    {
        ZeroPage(identifyVirt);
        uint sc = SubmitAdmin(NvmeAdminOp.Identify, nsid: nsid, prp1: identifyPhys, cdw10: NvmeCns.Namespace, cdw11: 0, cdw12: 0);
        if (sc != 0)
        {
            Serial.WriteString("[NVMe] Identify Namespace nsid=");
            Serial.WriteNumber(nsid);
            Serial.WriteString(" failed, status=0x");
            Serial.WriteHex(sc);
            Serial.WriteString("\n");
            return;
        }

        // NSZE (8 bytes, offset 0): namespace size in logical blocks.
        ulong nsze = *(ulong*)identifyVirt;
        if (nsze == 0)
        {
            return;
        }
        // FLBAS (1 byte, offset 26): bits [3:0] are the active LBA Format index.
        byte flbas = *(byte*)(identifyVirt + IdentifyNsFlbasOffset);
        int lbafIndex = flbas & FlbasFormatIndexMask;
        // LBAF entries start at offset 128, each 4 bytes: MS in bytes 0-1, LBADS in byte 2.
        ulong lbafEntry = identifyVirt + IdentifyNsLbafTableOffset + (ulong)(lbafIndex * LbafEntrySizeBytes);
        ushort metadataSize = *(ushort*)lbafEntry;
        byte lbads = *(byte*)(lbafEntry + LbafLbadsOffset);
        ulong blockSize = 1UL << lbads;

        // The single-PRP data path moves at most one 4 KiB page per command
        // and has no metadata handling: a larger LBA (or extended-LBA
        // metadata) would make the device DMA past the slot's bounce page
        // via PRP2=0, i.e. through physical address 0. Skip such
        // namespaces instead of corrupting memory.
        // NVMe 1.4 requires LBADS >= 9 (512-byte minimum): a corrupt or
        // zeroed LBAF entry would otherwise register a blockSize=1
        // namespace and partition scanning would issue nonsense
        // sub-sector I/O against it.
        if (lbads < MinSupportedLbads || metadataSize != 0 || blockSize > PageAllocator.PageSize)
        {
            Serial.WriteString("[NVMe] Skipping namespace nsid=");
            Serial.WriteNumber(nsid);
            Serial.WriteString(" (unsupported LBA format: blockSize=");
            Serial.WriteNumber(blockSize);
            Serial.WriteString(", metadata=");
            Serial.WriteNumber(metadataSize);
            Serial.WriteString(")\n");
            return;
        }

        Serial.WriteString("[NVMe] Namespace nsid=");
        Serial.WriteNumber(nsid);
        Serial.WriteString(" blocks=");
        Serial.WriteNumber(nsze);
        Serial.WriteString(" blockSize=");
        Serial.WriteNumber(blockSize);
        Serial.WriteString("\n");

        Namespaces.Add(new NvmeNamespace(this, nsid, nsze, blockSize));
    }

    private void CreateIoQueues()
    {
        _ioSqVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        _ioCqVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        _ioSqPhys = PageAllocator.VirtualToPhysical(_ioSqVirt);
        _ioCqPhys = PageAllocator.VirtualToPhysical(_ioCqVirt);
        _ioSqTail = 0;
        _ioCqHead = 0;
        _ioCqPhase = true;

        // Create IO Completion Queue first — Create IO SQ refers to it.
        // CDW10: bits [31:16] = qsize-1, bits [15:0] = qid
        // CDW11: bits [31:16] = IV (interrupt vector), bit 1 = IEN, bit 0 = PC
        uint cqCdw10 = ((IoQueueDepth - 1) << CreateQueueCdw10SizeShift) | IoQueueId;
        uint cqCdw11 = _msiXEnabled ? (((uint)_ioMsiXEntry << Cdw11IvShift) | Cdw11InterruptEnable | Cdw11PhysicallyContiguous) : Cdw11PhysicallyContiguous;
        uint sc = SubmitAdmin(NvmeAdminOp.CreateIoCq, nsid: 0, prp1: _ioCqPhys, cdw10: cqCdw10, cdw11: cqCdw11, cdw12: 0);
        if (sc != 0)
        {
            throw new Exception("[NVMe] Create IO CQ failed");
        }

        // Create IO Submission Queue. CDW11: bit 0 = PC, bits [2:1] = QPRIO (0=urgent),
        // bits [31:16] = CQID.
        uint sqCdw10 = ((IoQueueDepth - 1) << CreateQueueCdw10SizeShift) | IoQueueId;
        uint sqCdw11 = (IoQueueId << Cdw11CqidShift) | Cdw11PhysicallyContiguous; // CQID=1, PC=1
        sc = SubmitAdmin(NvmeAdminOp.CreateIoSq, nsid: 0, prp1: _ioSqPhys, cdw10: sqCdw10, cdw11: sqCdw11, cdw12: 0);
        if (sc != 0)
        {
            throw new Exception("[NVMe] Create IO SQ failed");
        }

        Serial.WriteString("[NVMe] IO queue pair created (qid=1, depth=");
        Serial.WriteNumber(IoQueueDepth);
        Serial.WriteString(")\n");
    }

    private static void ZeroPage(ulong virtAddr)
    {
        ulong* p = (ulong*)virtAddr;
        for (int i = 0; i < (int)(PageAllocator.PageSize / BytesPerUlong); i++)
        {
            p[i] = 0;
        }
    }

    private static void CopyIn(ReadOnlySpan<byte> src, ulong dstVirt)
    {
        byte* dst = (byte*)dstVirt;
        for (int i = 0; i < src.Length; i++)
        {
            dst[i] = src[i];
        }
    }

    private static void CopyOut(ulong srcVirt, Span<byte> dst)
    {
        byte* src = (byte*)srcVirt;
        for (int i = 0; i < dst.Length; i++)
        {
            dst[i] = src[i];
        }
    }
}
