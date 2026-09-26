// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver for the edu function that acquires what a real driver would,
/// a mapped BAR, a DMA buffer, bus mastering, polled interrupts, an event
/// and two work items, one of which it schedules, and publishes a mouse and
/// a network link, then throws from Probe. It keeps them, and the timer
/// polling its handler, so the teardown cells can check that the failed
/// attempt left none of them usable or running and that neither
/// publication reached its manager, and ranks first among the edu drivers
/// so every later candidate runs after its teardown.
/// </summary>
internal sealed class ThrowingDriver : PciDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "edu-throws";

    /// <summary>
    /// The last byte of the MAC address the failed attempt publishes its link
    /// with, after a locally administered 02:00:00:00:00 prefix no QEMU NIC
    /// uses, so the cells can tell the link apart from every real one.
    /// </summary>
    public const byte LinkAddressLastByte = 0xED;

    /// <summary>One page, like the edu driver's own buffer.</summary>
    private const int BufferLength = 4096;

    // Written from the interrupt handler, the driver-work thread and the
    // link's transmit handler, which must never run them: the attempt failed.
    private static int s_handlerCalls;
    private static int s_workRuns;
    private static int s_transmits;

    /// <summary>BAR 0 as the failed attempt mapped it, or null when the mapping failed.</summary>
    public static MmioRegion? Region { get; private set; }

    /// <summary>The DMA buffer the failed attempt allocated, or null when it got none.</summary>
    public static DmaBuffer? Buffer { get; private set; }

    /// <summary>What TryRequestInterrupts answered.</summary>
    public static bool InterruptsGranted { get; private set; }

    /// <summary>
    /// The timer the kit polled the failed attempt's handler from, read right
    /// after the request, or null when it had none. edu has no MSI-X, so a
    /// granted request is always polled.
    /// </summary>
    public static SoftwareTimer? PollTimer { get; private set; }

    /// <summary>The event the failed attempt created, or null when Probe never got that far.</summary>
    public static DeviceEvent? Event { get; private set; }

    /// <summary>The work item the failed attempt created and scheduled, or null when it got none.</summary>
    public static DeviceWorkItem? WorkItem { get; private set; }

    /// <summary>What scheduling <see cref="WorkItem"/> during Probe answered.</summary>
    public static bool WorkScheduled { get; private set; }

    /// <summary>
    /// A second work item the failed attempt created and never scheduled,
    /// or null when it got none. <see cref="WorkItem"/> is still pending
    /// after the teardown, so its Schedule would refuse even if the teardown
    /// had not dropped it; this one's refuses only because it did.
    /// </summary>
    public static DeviceWorkItem? UnscheduledWorkItem { get; private set; }

    /// <summary>The mouse the failed attempt published, or null when Probe never got that far.</summary>
    public static MouseReporter? Mouse { get; private set; }

    /// <summary>The network link the failed attempt published, or null when Probe never got that far.</summary>
    public static NetworkLink? Link { get; private set; }

    /// <summary>Calls of the failed attempt's transmit handler.</summary>
    public static int Transmits => Volatile.Read(ref s_transmits);

    /// <summary>Calls of the failed attempt's interrupt handler.</summary>
    public static int HandlerCalls => Volatile.Read(ref s_handlerCalls);

    /// <summary>Runs of the failed attempt's work item.</summary>
    public static int WorkRuns => Volatile.Read(ref s_workRuns);

    /// <inheritdoc />
    protected internal override ProbeResult Probe(PciDeviceContext context)
    {
        ProbeLog.Record(Name);

        if (context.TryMapBar(0, out MmioRegion? region))
        {
            Region = region;
        }

        if (context.TryAllocateDma(BufferLength, ulong.MaxValue, out DmaBuffer? buffer))
        {
            Buffer = buffer;
        }

        context.EnableBusMastering();
        InterruptsGranted = context.TryRequestInterrupts(OnInterrupt);
        PollTimer = PciContextInternals.PollTimer(context);
        Event = context.CreateEvent();
        if (context.TryCreateWorkItem(OnWork, out DeviceWorkItem? workItem))
        {
            WorkItem = workItem;
            WorkScheduled = workItem.Schedule();
        }

        if (context.TryCreateWorkItem(OnWork, out DeviceWorkItem? unscheduledWorkItem))
        {
            UnscheduledWorkItem = unscheduledWorkItem;
        }

        Mouse = context.PublishMouse();
        Link = context.PublishNetworkLink(new MACAddress([0x02, 0x00, 0x00, 0x00, 0x00, LinkAddressLastByte]), Transmit);

        throw new InvalidOperationException("the Drivers suite's throwing driver fails on purpose");
    }

    private static void OnInterrupt(int vector) => s_handlerCalls++;

    private static void OnWork() => Interlocked.Increment(ref s_workRuns);

    private static bool Transmit(ReadOnlySpan<byte> frame)
    {
        Interlocked.Increment(ref s_transmits);
        return false;
    }
}
