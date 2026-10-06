// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.Drivers.Pci.Storage.Nvme;

/// <summary>
/// The NVMe driver (NVM Express 1.4) over the driver kit: maps BAR0,
/// resets the controller, programs the admin queue pair in DMA memory,
/// identifies the controller and its active namespaces, allocates seven
/// command slots, connects message interrupt 1 (or 0 on a single-entry
/// table) when the platform routes it and polls otherwise, creates one
/// I/O queue pair and publishes every usable namespace to the ring as
/// <c>nvme{i}n{nsid}</c>. Everything it holds for one controller lives on an
/// <see cref="NvmeState"/> in <see cref="DeviceBinding.DriverState"/>. The
/// admin queue stays polled; the legacy line is never requested.
/// <see cref="Probe"/> and <see cref="OnDetach"/> run in thread context on
/// the kit worker.
/// </summary>
[Driver(Feature = DriverFeature.Storage)]
public sealed class NvmeDriver : Driver
{
    // --- Constants ---

    /// <summary>Mass storage controller, the class code of an NVMe function.</summary>
    private const byte MassStorageClass = 0x01;

    /// <summary>Non-volatile memory controller, the subclass.</summary>
    private const byte NonVolatileMemorySubclass = 0x08;

    /// <summary>NVM Express, the programming interface.</summary>
    private const byte NvmExpressProgIf = 0x02;

    /// <summary>The base address register holding the controller's registers and doorbells.</summary>
    private const int RegisterBar = 0;

    /// <summary>Bytes the register window has to span: the registers and the doorbell page.</summary>
    private const ulong RegisterWindowBytes = 0x2000;

    /// <summary>Doorbells the driver uses: the admin tail and head, the I/O tail and head.</summary>
    private const ulong DoorbellsUsed = 4;

    /// <summary>Bytes of one page: every queue, slot and scratch page is one, page aligned. The kit exposes no page size.</summary>
    private const int PageBytes = 4096;

    /// <summary>Bytes of a submission queue entry.</summary>
    private const int SqeBytes = 64;

    /// <summary>Bytes of a completion queue entry.</summary>
    private const int CqeBytes = 16;

    /// <summary>The message the I/O completion queue raises when the table has two or more entries: message 0 carries the admin completions and interrupts nobody.</summary>
    private const int PreferredMessage = 1;

    /// <summary>Bits of a shift that fit a 64-bit block size.</summary>
    private const int MaxLbaShift = 63;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: MassStorageClass, subclass: NonVolatileMemorySubclass, progIf: NvmExpressProgIf),
    ];

    /// <summary>The next controller number; probes are serialized on the kit worker, so no lock.</summary>
    private int _nextIndex;

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(NvmeDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up and publishes its namespaces. Thread
    /// context on the kit worker; a declined or failed result makes the kit
    /// release everything acquired here and quiet the function again.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the namespaces published; declined when BAR0 is not a memory window; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The wire structs, then BAR0 as the register window.
        if (Unsafe.SizeOf<NvmeSqe>() != SqeBytes || Unsafe.SizeOf<NvmeCqe>() != CqeBytes)
        {
            return ProbeResult.Failed("the wire structs have the wrong size");
        }

        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar0 = pci.Bars[RegisterBar];
        if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < RegisterWindowBytes)
        {
            return ProbeResult.Declined("BAR0 is not a memory window");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on; memory space before any message request,
        //    since the kit refuses a message while decode is off.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. The capabilities: the queue size, the doorbell stride, the
        //    ready budget.
        ulong cap = registers.Read64(NvmeProtocol.Cap);
        uint maxQueueEntries = (uint)(cap & NvmeProtocol.CapMqesMask);
        if (maxQueueEntries < NvmeProtocol.QueueDepth - 1)
        {
            return ProbeResult.Failed("the controller's queue size is below the driver's queue depth");
        }

        uint doorbellStride = (uint)((cap >> NvmeProtocol.CapDstrdShift) & NvmeProtocol.CapDstrdMask);
        uint timeoutUnits = (uint)((cap >> NvmeProtocol.CapToShift) & NvmeProtocol.CapToMask);
        if (NvmeProtocol.DoorbellBase + DoorbellsUsed * (NvmeProtocol.DoorbellStrideUnit << (int)doorbellStride) > registers.Length)
        {
            return ProbeResult.Failed("the doorbell stride does not fit BAR0");
        }

        NvmeState state = new(binding, registers, doorbellStride, timeoutUnits);

        // 4. Disable.
        if (!state.Disable())
        {
            return ProbeResult.Failed("the controller did not leave the ready state");
        }

        // 5. The admin queues.
        DmaBuffer adminSq = binding.AllocateDma(PageBytes, PageBytes);
        DmaBuffer adminCq = binding.AllocateDma(PageBytes, PageBytes);
        registers.Write32(NvmeProtocol.Aqa, NvmeProtocol.AqaValue);
        registers.Write64(NvmeProtocol.Asq, adminSq.PhysicalAddress);
        registers.Write64(NvmeProtocol.Acq, adminCq.PhysicalAddress);
        state.SetAdminQueues(adminSq, adminCq);

        // 6. Enable.
        registers.Write32(NvmeProtocol.Cc, NvmeProtocol.CcEnableValue);
        if (!state.WaitForReady(ready: true))
        {
            return ProbeResult.Failed("the controller did not become ready");
        }

        // 7. Every pin and MSI vector masked: the admin completions would
        //    otherwise assert the legacy line until the head doorbell write.
        registers.Write32(NvmeProtocol.Intms, NvmeProtocol.IntmsMaskAll);

        // 8. Identify: the controller, the active namespace list, then each
        //    namespace, through one scratch page.
        DmaBuffer scratch = binding.AllocateDma(PageBytes, PageBytes);
        string? failure = state.SubmitAdmin("identify controller", NvmeProtocol.AdminIdentify, 0, scratch.PhysicalAddress, NvmeProtocol.CnsController, 0, out ushort status);
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        if (status != 0)
        {
            binding.Log($"identify controller failed, status 0x{status:X}");
            return ProbeResult.Failed("identify controller failed");
        }

        scratch.Span.Clear();
        failure = state.SubmitAdmin("identify namespace list", NvmeProtocol.AdminIdentify, 0, scratch.PhysicalAddress, NvmeProtocol.CnsActiveNamespaceList, 0, out status);
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        if (status != 0)
        {
            binding.Log($"identify namespace list failed, status 0x{status:X}");
            return ProbeResult.Failed("identify namespace list failed");
        }

        // The ids leave the page before it is reused for the per-namespace
        // identifies.
        Span<uint> listed = MemoryMarshal.Cast<byte, uint>(scratch.Span);
        int idCount = 0;
        while (idCount < NvmeProtocol.MaxNamespaceIds && idCount < listed.Length && listed[idCount] != 0)
        {
            idCount++;
        }

        uint[] namespaceIds = new uint[idCount];
        listed.Slice(0, idCount).CopyTo(namespaceIds);

        List<NvmeNamespace> namespaces = [];
        for (int i = 0; i < namespaceIds.Length; i++)
        {
            uint nsid = namespaceIds[i];
            scratch.Span.Clear();
            failure = state.SubmitAdmin($"identify namespace {nsid}", NvmeProtocol.AdminIdentify, nsid, scratch.PhysicalAddress, NvmeProtocol.CnsNamespace, 0, out status);
            if (failure is not null)
            {
                return ProbeResult.Failed(failure);
            }

            if (status != 0)
            {
                binding.Log($"identify namespace {nsid} failed, status 0x{status:X}");
                continue;
            }

            Span<byte> page = scratch.Span;
            ulong blockCount = MemoryMarshal.Read<ulong>(page.Slice(NvmeProtocol.IdentifyNamespaceSizeOffset));
            if (blockCount == 0)
            {
                continue;
            }

            int formatIndex = page[NvmeProtocol.IdentifyNamespaceFlbasOffset] & NvmeProtocol.FlbasFormatIndexMask;
            int formatOffset = NvmeProtocol.IdentifyNamespaceLbafOffset + formatIndex * NvmeProtocol.LbafEntryBytes;
            ushort metadataBytes = MemoryMarshal.Read<ushort>(page.Slice(formatOffset));
            byte lbaShift = page[formatOffset + NvmeProtocol.LbafLbadsOffset];
            ulong blockSize = lbaShift > MaxLbaShift ? ulong.MaxValue : 1UL << lbaShift;
            if (lbaShift < NvmeProtocol.MinimumLbaShift || metadataBytes != 0 || blockSize > PageBytes)
            {
                binding.Log($"namespace {nsid} skipped (block size {blockSize}, metadata {metadataBytes})");
                continue;
            }

            namespaces.Add(new NvmeNamespace(state, nsid, blockCount, blockSize));
        }

        // 9. The slots and the lock.
        DmaBuffer[] pages = new DmaBuffer[NvmeProtocol.IoSlots];
        DeviceEvent[] events = new DeviceEvent[NvmeProtocol.IoSlots];
        for (int i = 0; i < NvmeProtocol.IoSlots; i++)
        {
            pages[i] = binding.AllocateDma(PageBytes, PageBytes);
            events[i] = binding.CreateEvent();
        }

        state.SetSlots(pages, events);
        state.Lock = binding.CreateLock();

        // 10. The interrupt: message 1 when the table has two entries or
        //     more, message 0 on a single-entry table, polling when the
        //     function has none or the platform cannot route it. The
        //     handler is live from here and gated until step 11.
        int messages = pci.MessageInterruptCount;
        int messageIndex = messages >= 2 ? PreferredMessage : (messages == 1 ? 0 : -1);
        bool connected = false;
        if (messageIndex >= 0 && binding.Node.Interrupts.Count > 1 + messageIndex)
        {
            connected = binding.TryRequestInterrupt(binding.Node.Interrupts[1 + messageIndex], state.OnInterrupt, out _);
        }

        state.HasInterrupt = connected;
        state.IsPolling = !connected;
        state.MessageIndex = connected ? messageIndex : -1;

        // 11. The I/O queue pair: the completion queue first, since the
        //     submission queue names it.
        DmaBuffer ioSq = binding.AllocateDma(PageBytes, PageBytes);
        DmaBuffer ioCq = binding.AllocateDma(PageBytes, PageBytes);
        state.SetIoQueues(ioSq, ioCq);
        uint cqWord11 = connected
            ? NvmeProtocol.CreateQueuePhysicallyContiguous | NvmeProtocol.CreateCqInterruptsEnabled | ((uint)messageIndex << NvmeProtocol.CreateCqVectorShift)
            : NvmeProtocol.CreateQueuePhysicallyContiguous;
        failure = state.SubmitAdmin("create I/O completion queue", NvmeProtocol.AdminCreateIoCq, 0, ioCq.PhysicalAddress, NvmeProtocol.CreateIoQueueCdw10, cqWord11, out status);
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        if (status != 0)
        {
            return ProbeResult.Failed("create I/O completion queue failed");
        }

        failure = state.SubmitAdmin("create I/O submission queue", NvmeProtocol.AdminCreateIoSq, 0, ioSq.PhysicalAddress, NvmeProtocol.CreateIoQueueCdw10, NvmeProtocol.CreateIoSqCdw11, out status);
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        if (status != 0)
        {
            return ProbeResult.Failed("create I/O submission queue failed");
        }

        state.IoQueueReady = true;

        // 12. The state, then the ring, one namespace at a time in id order.
        state.Index = _nextIndex++;
        state.NamespaceCount = namespaces.Count;
        binding.DriverState = state;
        for (int i = 0; i < namespaces.Count; i++)
        {
            binding.PublishBlockDevice(namespaces[i]);
        }

        // 13.
        binding.Log($"{state.NamespaceCount} namespaces, {(state.HasInterrupt ? "interrupt" : "polling")}, queue depth {NvmeProtocol.QueueDepth}");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Disables the controller (CC.EN clear, CSTS.RDY awaited within the
    /// CAP.TO budget, a miss ignored) and turns bus mastering off before the
    /// kit frees the queues and the slots. Thread context on the kit worker;
    /// the interrupt is already disconnected and the events cancelled, so a
    /// caller mid-command sees the detached exception; nothing is written
    /// when the hardware is gone.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not NvmeState state || !reason.HardwarePresent)
        {
            return;
        }

        state.Disable();
        binding.Node.Access<PciAccess>().EnableBusMastering(false);
    }
}
