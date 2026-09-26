// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for QEMU's NVMe controller (1b36:0010) that acquires
/// interrupts, a large DMA buffer and bus mastering, then returns Failed.
/// Its device match outranks the class match of <see cref="NvmeDriver"/>,
/// registered before it, so the NVMe cells see the kit tear this attempt
/// down and hand the same function to the next candidate: the MSI-X
/// vector, the DMA pages and MSI-X Enable must all be back as they were,
/// and a poll timer, where the kit polled instead, stopped.
/// </summary>
internal sealed class FailingNvmeDriver : PciDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "nvme-fails";

    /// <summary>QEMU's PCI vendor ID for its own devices (Red Hat).</summary>
    public const ushort VendorId = 0x1B36;

    /// <summary>QEMU's NVMe controller.</summary>
    public const ushort DeviceId = 0x0010;

    /// <summary>
    /// Pages of DMA memory the attempt takes, which its teardown must give
    /// back: far more than the heap grows by between this Probe and the
    /// next, so a leak shows in the free page count.
    /// </summary>
    public const int LeakCheckPages = 64;

    private const int PageSize = 4096;
    private const int RegisterBar = 0;

    // Written from the interrupt handler, which must never run: the attempt fails.
    private static int s_handlerCalls;

    /// <summary>True once Probe ran.</summary>
    public static bool Probed { get; private set; }

    /// <summary>Free pages when Probe began, before it allocated anything.</summary>
    public static ulong FreePagesBefore { get; private set; }

    /// <summary>Bound dynamic vectors when Probe began.</summary>
    public static int BoundVectorsBefore { get; private set; }

    /// <summary>What TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>
    /// The timer the kit polled the handler from, read right after the
    /// request, or null when the request went through MSI-X or was refused.
    /// </summary>
    public static SoftwareTimer? PollTimer { get; private set; }

    /// <summary>Bound dynamic vectors right after the request.</summary>
    public static int BoundVectorsAfterRequest { get; private set; }

    /// <summary>MSI-X Enable in the function's capability right after the request.</summary>
    public static bool MsiXEnabledAfterRequest { get; private set; }

    /// <summary>True when entry 0 of the MSI-X table could be read after the request.</summary>
    public static bool Entry0Read { get; private set; }

    /// <summary>Entry 0's mask bit right after the request.</summary>
    public static bool Entry0MaskedAfterRequest { get; private set; }

    /// <summary>True when the <see cref="LeakCheckPages"/>-page DMA buffer was allocated.</summary>
    public static bool DmaAllocated { get; private set; }

    /// <summary>Calls of the failed attempt's interrupt handler.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <inheritdoc />
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);
        Probed = true;
        FreePagesBefore = KernelState.FreePages;
        BoundVectorsBefore = KernelState.BoundVectors;

        if (!context.TryMapBar(RegisterBar, out _))
        {
            context.WriteLog("BAR 0 did not map");
        }

        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        PollTimer = PciContextInternals.PollTimer(context);
        BoundVectorsAfterRequest = KernelState.BoundVectors;
        MsiXEnabledAfterRequest = MsiXState.IsEnabled(context.Function);
        if (MsiXState.TryMapEntry0Control(context, out MmioRegion? table, out ulong entryControl))
        {
            Entry0Read = true;
            Entry0MaskedAfterRequest = (table.Read32(entryControl) & MsiXState.EntryMaskBit) != 0;
        }

        DmaAllocated = context.TryAllocateDma(LeakCheckPages * PageSize, ulong.MaxValue, out _);
        context.EnableBusMastering();
        return ProbeResult.Failed;
    }

    private static void OnInterrupt(int vector) => s_handlerCalls++;
}
