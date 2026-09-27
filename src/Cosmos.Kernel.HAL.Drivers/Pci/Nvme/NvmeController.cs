// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme.Commands;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme.Registers;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme;

/// <summary>
/// One NVMe controller as <see cref="NvmeDriver"/> brings it up: its
/// registers, its admin queue pair, one I/O queue pair, and the namespaces
/// it reported. Admin commands run during Probe only, one at a time, and
/// are polled. I/O commands complete through the I/O completion queue,
/// which whoever gets there first drains under one lock: the interrupt
/// handler, or a thread waiting for its command. A waiting thread leaves
/// the draining to the handler, and gives the CPU up, once the handler has
/// run behind an MSI-X vector. Until then, which covers the partition scan
/// of the disks the kit delivers before it arms the handler, and for good
/// when the kit could only poll the handler from the timer or grant no
/// interrupt at all, the thread drains the queue itself, so no command
/// waits for a timer tick.
///
/// Limitations:
/// <list type="bullet">
/// <item>One I/O submission and one I/O completion queue (queue ID 1),
/// depth 8. At most depth-1 commands are in flight (the NVMe queue-full
/// rule: a depth-N queue holds N-1 entries, or the wrapped tail is
/// indistinguishable from an empty queue).</item>
/// <item>Single-PRP transfers: every command moves one logical block
/// through its slot's bounce page, so PRP2 is always 0. Namespaces whose
/// LBA format exceeds one page, or carries metadata, are skipped at
/// discovery.</item>
/// <item>One interrupt vector, the one the kit grants: the I/O completion
/// queue shares vector 0 with the admin completion queue, whose commands
/// are all issued before the kit arms the handler.</item>
/// </list>
/// </summary>
internal sealed class NvmeController
{
    private const uint AdminQueueDepth = 8;
    private const uint IoQueueDepth = 8;
    private const uint AdminQueueId = 0;
    private const uint IoQueueId = 1;

    /// <summary>Bytes of every queue, the identify page and each bounce page: one page, the kit's DMA allocation unit.</summary>
    private const int PageBytes = 4096;

    /// <summary>CC.EN: controller enable (bit 0).</summary>
    private const uint CcEnable = 1u;
    /// <summary>CC.IOSQES: log2 of the I/O submission queue entry size (6: 64 bytes).</summary>
    private const uint CcIoSqes = 6u;
    /// <summary>Bit position of CC.IOSQES (bits 19:16).</summary>
    private const int CcIoSqesShift = 16;
    /// <summary>CC.IOCQES: log2 of the I/O completion queue entry size (4: 16 bytes).</summary>
    private const uint CcIoCqes = 4u;
    /// <summary>Bit position of CC.IOCQES (bits 23:20).</summary>
    private const int CcIoCqesShift = 20;
    /// <summary>CSTS.RDY: the controller is ready (bit 0).</summary>
    private const uint CstsReady = 1u;
    /// <summary>Bit position of AQA.ACQS, the admin completion queue size (bits 27:16).</summary>
    private const int AqaAcqsShift = 16;

    /// <summary>CAP.TO unit in milliseconds (NVMe 1.4 s3.1.1: TO counts 500 ms units).</summary>
    private const uint CapToUnitMilliseconds = 500;

    /// <summary>Milliseconds between two polls of CSTS.RDY.</summary>
    private const long ReadyPollMilliseconds = 1;

    /// <summary>
    /// Seconds a command may take to complete, whether its thread polls the
    /// completion queue or waits for the interrupt, and a caller may wait
    /// for a slot with none coming free, before it gives up: the I/O timeout
    /// Linux's NVMe driver uses by default. A hang-breaker for a lost
    /// message or a wedged controller, not a performance bound. Measured on
    /// the Stopwatch rather than counted in polls, whose length depends on
    /// the loop around them. Built into a TimeSpan where it is used: a
    /// static TimeSpan field would need a class constructor.
    /// </summary>
    private const long CompletionTimeoutSeconds = 30;

    /// <summary>Mask selecting SLBA[31:0] for CDW10 (NVMe 1.4 s6.9).</summary>
    private const ulong LbaLowDwordMask = 0xFFFFFFFF;
    /// <summary>Shift extracting SLBA[63:32] for CDW11.</summary>
    private const int LbaHighDwordShift = 32;
    /// <summary>Bit position of the 0-based queue size in Create I/O Completion/Submission Queue CDW10 (bits 31:16).</summary>
    private const int CreateQueueSizeShift = 16;
    /// <summary>IEN: interrupts enabled, in Create I/O Completion Queue CDW11 (bit 1). IV, bits 31:16, stays 0: the one vector the kit grants.</summary>
    private const uint Cdw11InterruptsEnabled = 1u << 1;
    /// <summary>PC: physically contiguous, in Create I/O Completion/Submission Queue CDW11 (bit 0).</summary>
    private const uint Cdw11PhysicallyContiguous = 1u;
    /// <summary>Bit position of the completion queue ID in Create I/O Submission Queue CDW11 (CQID, bits 31:16).</summary>
    private const int Cdw11CompletionQueueIdShift = 16;

    /// <summary>Most namespace IDs the Identify Active Namespace List returns: one page of 32-bit IDs.</summary>
    private const int MaxActiveNamespaceIds = PageBytes / sizeof(uint);
    /// <summary>Byte offset of FLBAS in the Identify Namespace data (NVMe 1.4 s5.15.2).</summary>
    private const int IdentifyNamespaceFlbasOffset = 26;
    /// <summary>FLBAS bits 3:0: the index of the LBA format in use.</summary>
    private const int FlbasFormatIndexMask = 0x0F;
    /// <summary>Byte offset of the LBA Format table in the Identify Namespace data.</summary>
    private const int IdentifyNamespaceLbafTableOffset = 128;
    /// <summary>Size of one LBA Format descriptor: MS in bytes 0-1, LBADS in byte 2.</summary>
    private const int LbafEntryBytes = 4;
    /// <summary>Byte offset of LBADS within an LBA Format descriptor.</summary>
    private const int LbafLbadsOffset = 2;
    /// <summary>Smallest LBADS NVMe 1.4 allows: 2^9, 512-byte logical blocks.</summary>
    private const int MinSupportedLbads = 9;
    /// <summary>Largest LBADS a bounce page holds: 2^12, 4096-byte logical blocks.</summary>
    private const int MaxSupportedLbads = 12;

    // MSI-X capability (PCI 3.0 s6.8.2): whether the kit's interrupts came
    // through it, read the way any driver can, from config space.
    private const byte MsiXCapabilityId = 0x11;
    private const ushort MsiXMessageControlOffset = 0x02;
    private const ushort MsiXEnableBit = 0x8000;

    private readonly ControllerRegisters _registers;
    private readonly List<NvmeNamespace> _namespaces = [];

    private readonly DmaBuffer _adminSubmissionQueue;
    private readonly DmaBuffer _adminCompletionQueue;
    private readonly DmaBuffer _identifyPage;
    private readonly DmaBuffer _ioSubmissionQueue;
    private readonly DmaBuffer _ioCompletionQueue;
    private readonly CommandSlot[] _slots;

    /// <summary>Counts the free slots: each Signal is one slot a caller may take.</summary>
    private readonly DeviceEvent _freeSlots;

    /// <summary>
    /// Slots released so far. A caller that waited out the timeout for a
    /// slot gives up only if none was released meanwhile: one that was went
    /// to a thread that asked first, which is progress, not a stuck
    /// controller.
    /// </summary>
    private long _slotReleases;

    /// <summary>
    /// Guards the slots' InUse flags, the I/O submission queue's tail and
    /// the I/O completion queue's head and phase, and is the lock the
    /// interrupt handler drains the queue under. Held briefly, never across
    /// a wait.
    /// </summary>
    private readonly IrqSafeLock _lock = new();

    // Admin queue pair: Probe only, one command at a time.
    private uint _adminSubmissionTail;
    private uint _adminCompletionHead;
    private bool _adminCompletionPhase = true;
    private ushort _nextAdminCommandId;

    // I/O queue pair, guarded by _lock.
    private uint _ioSubmissionTail;
    private uint _ioCompletionHead;
    private bool _ioCompletionPhase = true;

    /// <summary>
    /// True when the kit delivers the controller's interrupts through
    /// MSI-X, so a completion interrupts at once; false when it polls the
    /// handler from the timer, or granted no interrupt.
    /// </summary>
    private bool _messageSignaled;

    /// <summary>
    /// Set by the handler's first call. The kit calls the handler only once
    /// Probe returned Bound and it armed the vector, so from then on a
    /// thread can leave the draining to it.
    /// </summary>
    private volatile bool _handlerArmed;

    /// <summary>The binding attempt the controller belongs to, which logs its lines and busy-waits for it.</summary>
    internal PciDeviceContext Context { get; }

    /// <summary>The controller's number among the NVMe controllers probed, which names its namespaces (<c>nvme0n1</c>).</summary>
    internal int Index { get; }

    /// <summary>The namespaces <see cref="Initialize"/> found usable, in namespace ID order.</summary>
    internal IReadOnlyList<NvmeNamespace> Namespaces => _namespaces;

    private NvmeController(PciDeviceContext context, ControllerRegisters registers, int index,
        DmaBuffer adminSubmissionQueue, DmaBuffer adminCompletionQueue, DmaBuffer identifyPage,
        DmaBuffer ioSubmissionQueue, DmaBuffer ioCompletionQueue, CommandSlot[] slots, DeviceEvent freeSlots)
    {
        Context = context;
        Index = index;
        _registers = registers;
        _adminSubmissionQueue = adminSubmissionQueue;
        _adminCompletionQueue = adminCompletionQueue;
        _identifyPage = identifyPage;
        _ioSubmissionQueue = ioSubmissionQueue;
        _ioCompletionQueue = ioCompletionQueue;
        _slots = slots;
        _freeSlots = freeSlots;
    }

    /// <summary>
    /// Allocates everything the controller works in: the admin and I/O
    /// queues, the identify page, and each slot's bounce page and completion
    /// event. Probe only. All of it belongs to <paramref name="context"/>,
    /// which frees it if the attempt fails.
    /// </summary>
    /// <param name="context">The binding attempt.</param>
    /// <param name="registers">The mapped BAR 0.</param>
    /// <param name="index">The controller's number, which names its namespaces.</param>
    /// <param name="controller">The controller when the call returns true.</param>
    /// <returns>False, logged, when DMA memory ran out.</returns>
    internal static bool TryCreate(PciDeviceContext context, MmioRegion registers, int index, [NotNullWhen(true)] out NvmeController? controller)
    {
        controller = null;
        if (!TryAllocatePage(context, "the admin submission queue", out DmaBuffer? adminSubmissionQueue)
            || !TryAllocatePage(context, "the admin completion queue", out DmaBuffer? adminCompletionQueue)
            || !TryAllocatePage(context, "the identify data", out DmaBuffer? identifyPage)
            || !TryAllocatePage(context, "the I/O submission queue", out DmaBuffer? ioSubmissionQueue)
            || !TryAllocatePage(context, "the I/O completion queue", out DmaBuffer? ioCompletionQueue))
        {
            return false;
        }

        // Depth-1 slots: a depth-N submission queue holds at most N-1
        // outstanding entries (NVMe 1.4 s4.1) - with N in flight the wrapped
        // tail equals the head and the controller reads the queue as empty,
        // silently losing a command.
        CommandSlot[] slots = new CommandSlot[IoQueueDepth - 1];
        for (int i = 0; i < slots.Length; i++)
        {
            if (!TryAllocatePage(context, "an I/O bounce buffer", out DmaBuffer? bounceBuffer))
            {
                return false;
            }

            slots[i] = new CommandSlot((ushort)i, bounceBuffer, context.CreateEvent());
        }

        controller = new NvmeController(context, new ControllerRegisters(registers), index,
            adminSubmissionQueue, adminCompletionQueue, identifyPage, ioSubmissionQueue, ioCompletionQueue,
            slots, context.CreateEvent());
        return true;
    }

    /// <summary>
    /// Brings the controller up: reset, admin queues, identify the
    /// controller and its namespaces, request interrupts, and create the I/O
    /// queue pair. Probe only.
    /// </summary>
    /// <returns>False, logged, when the controller cannot be driven by this driver.</returns>
    /// <exception cref="IOException">The controller did not become ready, or an admin command timed out or failed.</exception>
    internal bool Initialize()
    {
        if (!_registers.CoversDoorbellsOf(IoQueueId))
        {
            Context.WriteLog("BAR 0 is too small to hold the I/O queue's doorbells");
            return false;
        }

        // NVMe 1.4: a queue may not exceed CAP.MQES+1 entries. Our fixed
        // depth is tiny (8), but a controller reporting less would silently
        // get an out-of-spec queue size (Invalid Queue Size on create, or
        // worse): the function is left unbound instead.
        if (_registers.MaximumQueueEntries < IoQueueDepth)
        {
            Context.WriteLog($"the controller's queues hold at most {_registers.MaximumQueueEntries} entries, fewer than the driver's {IoQueueDepth}");
            return false;
        }

        DisableController();
        SetupAdminQueues();

        // Only now, as the kit asks: the controller is reset and the only
        // addresses it holds are the admin queues just programmed. Turned on
        // earlier, a controller firmware left running, or one a failed
        // attempt left enabled, could still write into pages the kernel
        // reuses before the reset stops it.
        Context.EnableBusMastering();
        EnableController();

        // Mask every pin-based interrupt vector: admin completions (and all
        // I/O when no MSI-X is granted) would otherwise assert the
        // level-triggered INTx line until the CQ head doorbell write, a
        // spurious-interrupt source on any platform where that line is
        // unmasked or shared. The kit already turned INTx off in the
        // Command register; MSI-X, requested below, is unaffected by INTMS,
        // which must not be touched once it is on.
        _registers.MaskAllVectors();

        Context.WriteLog("controller ready");

        DiscoverNamespaces();
        SetupIoCompletion();
        CreateIoQueues();
        return true;
    }

    /// <summary>
    /// Disables the controller after a failed <see cref="Initialize"/>, best
    /// effort: the kit frees the queues it was given once Probe returns
    /// Failed, and an enabled controller still holds their addresses, which
    /// only bus mastering being off would keep it from writing to. Probe
    /// only; a controller that does not stop is logged and left as it is.
    /// </summary>
    internal void Quiesce()
    {
        try
        {
            DisableController();
        }
        catch (IOException exception)
        {
            Context.WriteLog($"the controller did not stop after the failed probe: {exception.Message}");
        }
    }

    /// <summary>
    /// Reads one logical block of namespace <paramref name="namespaceId"/>
    /// at <paramref name="lba"/> into <paramref name="block"/>, which is as
    /// long as the namespace's block. Thread-safe: callers run on slots of
    /// their own, up to <see cref="IoQueueDepth"/>-1 at once. Thread context
    /// only, once the binding is Bound.
    /// </summary>
    /// <returns>The completion's status code: 0 for success.</returns>
    /// <exception cref="IOException">No slot freed up, or the command never completed, within the timeout.</exception>
    internal uint Read(uint namespaceId, ulong lba, Span<byte> block)
    {
        ThrowIfNotOneBlock(block.Length, nameof(block));
        CommandSlot slot = AcquireSlot();
        try
        {
            uint status = Execute(IoOpcode.Read, namespaceId, slot, lba);
            if (status == 0)
            {
                slot.BounceBuffer.Span[..block.Length].CopyTo(block);
            }

            ReleaseSlot(slot);
            return status;
        }
        catch
        {
            QuarantineSlot(slot);
            throw;
        }
    }

    /// <summary>
    /// Writes <paramref name="block"/>, one logical block, to namespace
    /// <paramref name="namespaceId"/> at <paramref name="lba"/>. Thread-safe,
    /// as <see cref="Read"/> is.
    /// </summary>
    /// <returns>The completion's status code: 0 for success.</returns>
    /// <exception cref="IOException">No slot freed up, or the command never completed, within the timeout.</exception>
    internal uint Write(uint namespaceId, ulong lba, ReadOnlySpan<byte> block)
    {
        ThrowIfNotOneBlock(block.Length, nameof(block));
        CommandSlot slot = AcquireSlot();
        try
        {
            block.CopyTo(slot.BounceBuffer.Span);
            uint status = Execute(IoOpcode.Write, namespaceId, slot, lba);

            ReleaseSlot(slot);
            return status;
        }
        catch
        {
            QuarantineSlot(slot);
            throw;
        }
    }

    /// <summary>Flushes the volatile write cache for namespace <paramref name="namespaceId"/>. Thread-safe, as <see cref="Read"/> is.</summary>
    /// <returns>The completion's status code: 0 for success.</returns>
    /// <exception cref="IOException">No slot freed up, or the command never completed, within the timeout.</exception>
    internal uint Flush(uint namespaceId)
    {
        CommandSlot slot = AcquireSlot();
        try
        {
            uint status = Execute(IoOpcode.Flush, namespaceId, slot, 0);

            ReleaseSlot(slot);
            return status;
        }
        catch
        {
            QuarantineSlot(slot);
            throw;
        }
    }

    private static bool TryAllocatePage(PciDeviceContext context, string purpose, [NotNullWhen(true)] out DmaBuffer? page)
    {
        // NVMe carries 64-bit addresses in every queue base and PRP entry.
        if (context.TryAllocateDma(PageBytes, ulong.MaxValue, out page))
        {
            return true;
        }

        context.WriteLog($"no DMA memory for {purpose}");
        return false;
    }

    /// <summary>
    /// The single-PRP data path moves at most one page per command, and the
    /// namespace hands over exactly one logical block, which discovery
    /// capped at a page. Anything else would make the device transfer a
    /// length the span does not describe.
    /// </summary>
    private static void ThrowIfNotOneBlock(int byteLength, string paramName)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteLength, PageBytes, paramName);
    }

    /// <summary>The Stopwatch timestamp a command started now must complete by.</summary>
    private static long CompletionDeadline() =>
        Stopwatch.GetTimestamp() + CompletionTimeoutSeconds * Stopwatch.Frequency;

    /// <summary>True when the function's MSI-X capability has MSI-X Enable set, which the kit sets when it grants MSI-X.</summary>
    private static bool IsMsiXEnabled(PciFunction function) =>
        function.TryFindCapability(MsiXCapabilityId, out ushort capability)
        && (function.ReadConfig16((ushort)(capability + MsiXMessageControlOffset)) & MsiXEnableBit) != 0;

    private void DisableController()
    {
        uint configuration = _registers.Configuration;
        if ((configuration & CcEnable) != 0)
        {
            _registers.Configuration = configuration & ~CcEnable;
        }

        WaitForReady(false);
    }

    private void EnableController()
    {
        // CC.IOSQES=6 (64-byte SQE), CC.IOCQES=4 (16-byte CQE), CC.MPS=0 (4K), CC.CSS=0, EN=1
        _registers.Configuration = (CcIoSqes << CcIoSqesShift) | (CcIoCqes << CcIoCqesShift) | CcEnable;

        WaitForReady(true);
    }

    // NVMe 1.4 s3.1.1: software must wait up to CAP.TO (500 ms units, up
    // to 63.5 s) for CSTS.RDY to track CC.EN. A fixed spin would be ~0.3-2 s
    // depending on read latency, and healthy drives with slow bring-up
    // would spuriously fail init and be skipped.
    private void WaitForReady(bool ready)
    {
        // At least one 500 ms unit even if a controller reports TO=0.
        uint timeoutUnits = _registers.ReadyTimeoutUnits;
        uint budgetMilliseconds = (timeoutUnits == 0 ? 1 : timeoutUnits) * CapToUnitMilliseconds;
        for (uint elapsedMilliseconds = 0; ; elapsedMilliseconds++)
        {
            if (((_registers.Status & CstsReady) != 0) == ready)
            {
                return;
            }

            if (elapsedMilliseconds >= budgetMilliseconds)
            {
                throw new IOException(ready ? "NVMe: timeout waiting for CSTS.RDY=1." : "NVMe: timeout waiting for CSTS.RDY=0.");
            }

            Context.Delay(TimeSpan.FromMilliseconds(ReadyPollMilliseconds));
        }
    }

    private void SetupAdminQueues()
    {
        // AQA: ACQS in [27:16], ASQS in [11:0], both 0-based.
        uint attributes = ((AdminQueueDepth - 1) << AqaAcqsShift) | (AdminQueueDepth - 1);
        _registers.SetAdminQueues(attributes, _adminSubmissionQueue.DeviceAddress, _adminCompletionQueue.DeviceAddress);
    }

    /// <summary>
    /// Submits an admin command and polls for its completion. Probe only.
    /// </summary>
    /// <returns>The completion's status code: 0 for success.</returns>
    /// <exception cref="IOException">The command never completed.</exception>
    private uint SubmitAdmin(AdminOpcode opcode, uint namespaceId, ulong dataPointer, uint commandDword10, uint commandDword11)
    {
        ushort commandId = _nextAdminCommandId++;

        SubmissionEntry entry = new(_adminSubmissionQueue.Span, _adminSubmissionTail);
        entry.SetCommand(opcode, commandId);
        entry.SetNamespace(namespaceId);
        entry.SetDataPointer(dataPointer);
        entry.SetCommandDword10(commandDword10);
        entry.SetCommandDword11(commandDword11);

        _adminSubmissionTail = (_adminSubmissionTail + 1) % AdminQueueDepth;

        // The register write orders the entry's stores (DMA memory) before
        // the doorbell store (device memory): ARM64 does not order the two
        // on its own, so the controller could otherwise fetch a half-written
        // command.
        _registers.RingSubmissionDoorbell(AdminQueueId, _adminSubmissionTail);

        return WaitForAdminCompletion(commandId);
    }

    private uint WaitForAdminCompletion(ushort commandId)
    {
        ReadOnlySpan<byte> queue = _adminCompletionQueue.Span;
        long deadline = CompletionDeadline();
        while (true)
        {
            CompletionEntry entry = new(queue, _adminCompletionHead);
            if (entry.Phase == _adminCompletionPhase)
            {
                // Read barrier: the phase bit is device-written; the rest of
                // the entry (and the DMA'd data it guards) must not be read
                // ahead of it on weakly-ordered ARM64.
                DmaBuffer.ReadBarrier();

                uint status = entry.StatusCode;
                ushort completedId = entry.CommandId;

                _adminCompletionHead = (_adminCompletionHead + 1) % AdminQueueDepth;
                if (_adminCompletionHead == 0)
                {
                    _adminCompletionPhase = !_adminCompletionPhase;
                }

                _registers.RingCompletionDoorbell(AdminQueueId, _adminCompletionHead);

                if (completedId != commandId)
                {
                    // Stale entry from an abandoned command (an earlier
                    // timeout): consume it and keep waiting for the expected
                    // identifier instead of misattributing its status.
                    Context.WriteLog($"consumed a stale admin completion (command {completedId}, expected {commandId})");
                    continue;
                }

                return status;
            }

            if (Stopwatch.GetTimestamp() > deadline)
            {
                throw new IOException("NVMe: timeout waiting for an admin command to complete.");
            }
        }
    }

    private void DiscoverNamespaces()
    {
        // Identify Controller (CNS=0x01): only used to confirm the controller is responsive.
        uint status = SubmitAdmin(AdminOpcode.Identify, 0, _identifyPage.DeviceAddress, (uint)IdentifyCns.Controller, 0);
        if (status != 0)
        {
            Context.WriteLog($"Identify Controller failed, status 0x{status:X}");
            return;
        }

        // Identify Active Namespace List (CNS=0x02): up to 1024 IDs.
        _identifyPage.Span.Clear();
        status = SubmitAdmin(AdminOpcode.Identify, 0, _identifyPage.DeviceAddress, (uint)IdentifyCns.ActiveNamespaceList, 0);
        if (status != 0)
        {
            Context.WriteLog($"Identify Active Namespace List failed, status 0x{status:X}");
            return;
        }

        // Copied out before the page is reused for each namespace's own
        // Identify, which overwrites it.
        uint[] namespaceIds = ReadActiveNamespaceIds(_identifyPage.Span);
        for (int i = 0; i < namespaceIds.Length; i++)
        {
            RegisterNamespace(namespaceIds[i]);
        }
    }

    /// <summary>The IDs of an Active Namespace List, which ends at the first zero or at the page's end.</summary>
    private static uint[] ReadActiveNamespaceIds(ReadOnlySpan<byte> list)
    {
        int count = 0;
        while (count < MaxActiveNamespaceIds && BinaryPrimitives.ReadUInt32LittleEndian(list[(count * sizeof(uint))..]) != 0)
        {
            count++;
        }

        uint[] ids = new uint[count];
        for (int i = 0; i < count; i++)
        {
            ids[i] = BinaryPrimitives.ReadUInt32LittleEndian(list[(i * sizeof(uint))..]);
        }

        return ids;
    }

    private void RegisterNamespace(uint namespaceId)
    {
        _identifyPage.Span.Clear();
        uint status = SubmitAdmin(AdminOpcode.Identify, namespaceId, _identifyPage.DeviceAddress, (uint)IdentifyCns.Namespace, 0);
        if (status != 0)
        {
            Context.WriteLog($"Identify Namespace {namespaceId} failed, status 0x{status:X}");
            return;
        }

        ReadOnlySpan<byte> identify = _identifyPage.Span;

        // NSZE (8 bytes, offset 0): namespace size in logical blocks.
        ulong blockCount = BinaryPrimitives.ReadUInt64LittleEndian(identify);
        if (blockCount == 0)
        {
            return;
        }

        // FLBAS bits 3:0 pick the LBA format in use; each format descriptor
        // holds MS (the metadata size) in bytes 0-1 and LBADS in byte 2.
        int formatIndex = identify[IdentifyNamespaceFlbasOffset] & FlbasFormatIndexMask;
        ReadOnlySpan<byte> format = identify.Slice(IdentifyNamespaceLbafTableOffset + formatIndex * LbafEntryBytes, LbafEntryBytes);
        ushort metadataBytes = BinaryPrimitives.ReadUInt16LittleEndian(format);
        byte lbads = format[LbafLbadsOffset];

        // The single-PRP data path moves at most one page per command and
        // has no metadata handling: a larger block (or extended-LBA
        // metadata) would make the device DMA past the slot's bounce page
        // via PRP2=0, i.e. through physical address 0. NVMe 1.4 also
        // requires LBADS >= 9: a corrupt or zeroed format entry would
        // otherwise register a 1-byte-block namespace and partition scanning
        // would issue nonsense sub-sector I/O against it. The range is
        // checked on LBADS itself, before the shift, which a value of 64 or
        // more would wrap.
        if (lbads < MinSupportedLbads || lbads > MaxSupportedLbads || metadataBytes != 0)
        {
            Context.WriteLog($"skipping namespace {namespaceId}: unsupported LBA format (LBADS {lbads}, {metadataBytes} metadata bytes)");
            return;
        }

        ulong blockSize = 1UL << lbads;
        Context.WriteLog($"namespace {namespaceId}: {blockCount} blocks of {blockSize} bytes");
        _namespaces.Add(new NvmeNamespace(this, namespaceId, blockCount, blockSize));
    }

    /// <summary>
    /// Makes every slot free and asks the kit for the controller's
    /// interrupts. With MSI-X, completions interrupt through vector 0;
    /// polled from the timer, or not granted, the waiting threads drain the
    /// queue themselves.
    /// </summary>
    private void SetupIoCompletion()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _freeSlots.Signal();
        }

        _messageSignaled = Context.TryRequestInterrupts(OnInterrupt) && IsMsiXEnabled(Context.Function);
        Context.WriteLog(_messageSignaled
            ? "I/O completions through MSI-X vector 0"
            : "I/O completions polled by the waiting thread");
    }

    private void CreateIoQueues()
    {
        // Create I/O Completion Queue first: Create I/O Submission Queue
        // refers to it. CDW10: 0-based size in 31:16, queue ID in 15:0.
        // CDW11: IV in 31:16 (0), IEN in bit 1, PC in bit 0.
        uint queueSize = ((IoQueueDepth - 1) << CreateQueueSizeShift) | IoQueueId;
        uint completionAttributes = _messageSignaled
            ? Cdw11InterruptsEnabled | Cdw11PhysicallyContiguous
            : Cdw11PhysicallyContiguous;
        uint status = SubmitAdmin(AdminOpcode.CreateIoCompletionQueue, 0, _ioCompletionQueue.DeviceAddress, queueSize, completionAttributes);
        if (status != 0)
        {
            throw new IOException($"NVMe: Create I/O Completion Queue failed, status 0x{status:X}.");
        }

        // Create I/O Submission Queue. CDW11: CQID in 31:16, QPRIO in 2:1
        // (0, urgent), PC in bit 0.
        uint submissionAttributes = (IoQueueId << Cdw11CompletionQueueIdShift) | Cdw11PhysicallyContiguous;
        status = SubmitAdmin(AdminOpcode.CreateIoSubmissionQueue, 0, _ioSubmissionQueue.DeviceAddress, queueSize, submissionAttributes);
        if (status != 0)
        {
            throw new IOException($"NVMe: Create I/O Submission Queue failed, status 0x{status:X}.");
        }

        Context.WriteLog($"I/O queue pair created (queue {IoQueueId}, depth {IoQueueDepth})");
    }

    /// <summary>
    /// Takes a free slot. A caller finding none waits for one to be
    /// released, giving the CPU up where the scheduler can park it.
    /// </summary>
    /// <exception cref="IOException">No slot freed up within the timeout: every slot may be quarantined.</exception>
    private CommandSlot AcquireSlot()
    {
        while (true)
        {
            long releasesBefore = Interlocked.Read(ref _slotReleases);
            if (_freeSlots.Wait(TimeSpan.FromSeconds(CompletionTimeoutSeconds)))
            {
                break;
            }

            if (Interlocked.Read(ref _slotReleases) == releasesBefore)
            {
                throw new IOException("NVMe: no I/O slot came free within the timeout.");
            }
        }

        using (_lock.EnterScope())
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].InUse)
                {
                    _slots[i].InUse = true;
                    return _slots[i];
                }
            }
        }

        // _freeSlots counts exactly the slots not in use, so a successful
        // wait always leaves one.
        throw new InvalidOperationException("NVMe: no free I/O slot although one was counted.");
    }

    private void ReleaseSlot(CommandSlot slot)
    {
        using (_lock.EnterScope())
        {
            slot.InUse = false;
        }

        Interlocked.Increment(ref _slotReleases);
        _freeSlots.Signal();
    }

    /// <summary>
    /// Called when a command failed in a way that may leave it outstanding
    /// in the controller (a timeout). Deliberately leaves the slot in use,
    /// and uncounted, so it is never handed out again: a straggling
    /// completion would otherwise be misattributed to a new command and
    /// DMA into a recycled buffer. The (rare) slot leak is the price of
    /// containment.
    /// </summary>
    private void QuarantineSlot(CommandSlot slot) =>
        Context.WriteLog($"quarantined I/O slot {slot.CommandId}: its command may still be outstanding");

    /// <summary>
    /// Builds the command in the submission queue for an acquired slot,
    /// rings the doorbell and waits for the completion. Thread-safe.
    /// </summary>
    /// <returns>The completion's status code: 0 for success.</returns>
    /// <exception cref="IOException">The command never completed within the timeout.</exception>
    private uint Execute(IoOpcode opcode, uint namespaceId, CommandSlot slot, ulong lba)
    {
        slot.Reset();

        // Decided before the doorbell: a thread that starts out draining the
        // queue itself keeps doing so, which stays correct whatever the
        // handler starts doing meanwhile, since both drain under the lock.
        bool waitForHandler = _messageSignaled && _handlerArmed;

        using (_lock.EnterScope())
        {
            SubmissionEntry entry = new(_ioSubmissionQueue.Span, _ioSubmissionTail);
            entry.SetCommand(opcode, slot.CommandId);
            entry.SetNamespace(namespaceId);
            entry.SetDataPointer(slot.BounceBuffer.DeviceAddress);
            entry.SetCommandDword10((uint)(lba & LbaLowDwordMask));
            entry.SetCommandDword11((uint)(lba >> LbaHighDwordShift));

            // CDW12 stays 0: NLB, the 0-based number of logical blocks, is one block.
            _ioSubmissionTail = (_ioSubmissionTail + 1) % IoQueueDepth;

            // The entry's stores must reach the device before the doorbell
            // does (see SubmitAdmin).
            _registers.RingSubmissionDoorbell(IoQueueId, _ioSubmissionTail);
        }

        if (waitForHandler)
        {
            WaitForHandler(slot);
        }
        else
        {
            DrainUntilDone(slot);
        }

        return slot.Status;
    }

    /// <summary>Waits for the interrupt handler to drain the slot's completion.</summary>
    private void WaitForHandler(CommandSlot slot)
    {
        // A signal can be left over from an earlier command on the slot,
        // one the handler (or a draining thread) sent after its waiter had
        // already seen it done, so the flag decides, and a stale signal only
        // costs one more wait.
        while (!slot.IsDone)
        {
            if (slot.Completed.Wait(TimeSpan.FromSeconds(CompletionTimeoutSeconds)))
            {
                continue;
            }

            // Messages may have stopped with the completion already in the
            // queue: drain it once before giving up on the command.
            using (_lock.EnterScope())
            {
                DrainCompletionsLocked();
            }

            if (!slot.IsDone)
            {
                // A lost or misrouted message surfaces as the same timeout as
                // a polled command (the caller quarantines the slot) instead
                // of parking the thread forever with no diagnostic.
                throw new IOException("NVMe: no completion interrupt within the timeout.");
            }
        }
    }

    /// <summary>Drains the completion queue from this thread until the slot's completion is in.</summary>
    private void DrainUntilDone(CommandSlot slot)
    {
        long deadline = CompletionDeadline();
        while (true)
        {
            using (_lock.EnterScope())
            {
                DrainCompletionsLocked();
            }

            if (slot.IsDone)
            {
                return;
            }

            if (Stopwatch.GetTimestamp() > deadline)
            {
                throw new IOException("NVMe: timeout waiting for command completion.");
            }
        }
    }

    /// <summary>
    /// The interrupt handler, behind MSI-X vector 0 or polled from the
    /// timer. Interrupt context: it allocates nothing, throws nothing and
    /// only takes the controller's IRQ-safe lock.
    /// </summary>
    private void OnInterrupt(int vector)
    {
        _handlerArmed = true;
        using (_lock.EnterScope())
        {
            DrainCompletionsLocked();
        }
    }

    /// <summary>
    /// Consumes every new entry of the I/O completion queue, records each
    /// one in its slot, and hands the controller the new head. The caller
    /// holds <see cref="_lock"/>. Any context: it allocates nothing.
    /// </summary>
    private void DrainCompletionsLocked()
    {
        ReadOnlySpan<byte> queue = _ioCompletionQueue.Span;
        bool drained = false;
        while (true)
        {
            CompletionEntry entry = new(queue, _ioCompletionHead);
            if (entry.Phase != _ioCompletionPhase)
            {
                break;
            }

            // Read barrier: don't consume the identifier and status (or the
            // DMA'd payload they guard) ahead of the device-written phase bit.
            DmaBuffer.ReadBarrier();

            ushort commandId = entry.CommandId;
            uint status = entry.StatusCode;

            _ioCompletionHead = (_ioCompletionHead + 1) % IoQueueDepth;
            if (_ioCompletionHead == 0)
            {
                _ioCompletionPhase = !_ioCompletionPhase;
            }

            drained = true;

            if (commandId < _slots.Length)
            {
                CommandSlot slot = _slots[commandId];
                slot.Complete(status);

                // Only with MSI-X does a thread ever wait on the event; the
                // polled modes would pile up signals nobody consumes.
                if (_messageSignaled)
                {
                    slot.Completed.Signal();
                }
            }
        }

        if (drained)
        {
            _registers.RingCompletionDoorbell(IoQueueId, _ioCompletionHead);
        }
    }
}
