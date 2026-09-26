// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// A driver matching HID interfaces with no boot subclass (03/00), which
/// QEMU's tablet presents, that publishes a network link with a transmit
/// handler that only counts frames, and binds. It ranks below the tablet
/// driver's exact match, so the tablet present at boot, whose offering that
/// driver ends, never reaches it: it binds the tablet once it is plugged in
/// again, and its link is what the cells watch leave the network manager
/// when the tablet is pulled out. Every binding publishes a MAC address of
/// its own, numbered by instance, so the cells tell the links apart.
/// </summary>
internal sealed class UsbTabletLinkDriver : UsbDriver
{
    /// <summary>The registration's name.</summary>
    public const string Name = "usb-tablet-link";

    // Written by the transmit handler, with interrupts masked, by Probe and
    // by Remove, and read by the cells.
    private static int s_instances;
    private static int s_transmits;
    private static int s_removeCalls;

    /// <summary>Instances the registration's factory created, one per offering of a tablet.</summary>
    public static int Instances => Volatile.Read(ref s_instances);

    /// <summary>Frames the stack sent through any of the driver's links.</summary>
    public static int Transmits => Volatile.Read(ref s_transmits);

    /// <summary>Calls of Remove, across every binding.</summary>
    public static int RemoveCalls => Volatile.Read(ref s_removeCalls);

    /// <summary>The context of the latest Probe, or null when none ran.</summary>
    public static UsbDeviceContext? Context { get; private set; }

    /// <summary>The link the latest Probe published, or null when none ran.</summary>
    public static NetworkLink? Link { get; private set; }

    /// <summary>The scheduler's ID for the thread the latest Probe ran on.</summary>
    public static uint ProbeThreadId { get; private set; }

    /// <summary>Whether the latest Probe ran on its CPU's idle thread, the thread that boots the kernel.</summary>
    public static bool ProbeOnIdleThread { get; private set; }

    /// <summary>What the context reported for IsPresent when the last Remove ran.</summary>
    public static bool PresentAtRemove { get; private set; }

    /// <summary>Whether the binding's link was already withdrawn when the last Remove ran.</summary>
    public static bool LinkWithdrawnAtRemove { get; private set; }

    /// <summary>
    /// True while a binding of the driver holds a tablet: from its Probe
    /// until its Remove.
    /// </summary>
    public static bool IsBound { get; private set; }

    /// <summary>Counts the instances the registration's factory created.</summary>
    public UsbTabletLinkDriver()
    {
        Interlocked.Increment(ref s_instances);
    }

    /// <summary>The MAC address the binding of instance <paramref name="instance"/> publishes: locally administered, its last byte the instance number.</summary>
    public static MACAddress AddressOf(int instance) => new MACAddress([0x02, 0x00, 0x00, 0x00, 0x7A, (byte)instance]);

    /// <summary>The registration the kernel passes to Register: HID with no boot subclass, whatever the protocol.</summary>
    public static UsbDriverRegistration CreateRegistration() =>
        new(Name, static () => new UsbTabletLinkDriver(),
            UsbMatch.Interface(UsbBootMouseDriver.HidClass, UsbTabletFailingDriver.NoSubclass));

    /// <inheritdoc />
    protected internal override ProbeResult Probe(UsbDeviceContext context)
    {
        ProbeLog.Record(Name, context.Path);
        Context = context;
        _ = KernelState.TryGetCurrentThread(out uint threadId, out bool isIdle);
        ProbeThreadId = threadId;
        ProbeOnIdleThread = isIdle;

        Link = context.PublishNetworkLink(AddressOf(Instances), Transmit);
        IsBound = true;
        return ProbeResult.Bound;
    }

    /// <summary>Records that the tablet left the bus, and whether the kit had withdrawn the link and cleared IsPresent by then.</summary>
    protected internal override void Remove(UsbDeviceContext context)
    {
        PresentAtRemove = context.IsPresent;
        LinkWithdrawnAtRemove = Link is { } link && link.Device.IsWithdrawn;
        IsBound = false;
        Interlocked.Increment(ref s_removeCalls);
    }

    /// <summary>The link's transmit handler: counts the frame and takes it. Interrupts are masked.</summary>
    private static bool Transmit(ReadOnlySpan<byte> frame)
    {
        s_transmits++;
        return true;
    }
}
