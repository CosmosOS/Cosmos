// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Devices.Network;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// A network link a registered driver published, as the network manager and
/// the stack see it: a <see cref="NetworkDevice"/> like the built-in ones.
/// The stack's sends go to the driver's <see cref="NetworkTransmitHandler"/>,
/// and the frames the driver delivers through its <see cref="NetworkLink"/>
/// go to the stack's <see cref="NetworkDevice.OnPacketReceived"/>.
/// </summary>
internal sealed class PublishedNetworkDevice : NetworkDevice
{
    private readonly DeviceContext _context;
    private readonly NetworkTransmitHandler _transmit;

    /// <summary>
    /// Serializes the driver's transmit handler and masks interrupts around
    /// it, as its contract promises. Not readonly: SpinLock is a mutable struct.
    /// </summary>
    private SchedSpinLock _transmitLock;

    /// <summary>Where the link stands; only <see cref="LinkState.Live"/> carries traffic.</summary>
    private volatile LinkState _state;

    /// <summary>Cleared by <see cref="Disable"/>; the link carries no traffic while it is.</summary>
    private volatile bool _enabled = true;

    /// <summary>What the driver last reported through <see cref="NetworkLink.SetLinkState"/>.</summary>
    private volatile bool _linkUp;

    /// <summary>
    /// Where a published link stands. It starts <see cref="Queued"/> and
    /// leaves that state once, for good.
    /// </summary>
    private enum LinkState
    {
        /// <summary>Published during Probe and held by the kit: nothing reaches it yet.</summary>
        Queued,

        /// <summary>Delivered to the network manager once Probe returned Bound.</summary>
        Live,

        /// <summary>Dropped with the attempt that published it, which was declined or failed.</summary>
        Gone
    }

    /// <summary>The driver's registered name and the device's path, such as <c>rtl8139 pci/0000:00:03.0</c>.</summary>
    public override string Name { get; }

    /// <summary>The MAC address the driver published the link with.</summary>
    public override MACAddress MacAddress { get; }

    /// <summary>What the driver last reported through <see cref="NetworkLink.SetLinkState"/>; false until it does.</summary>
    public override bool LinkUp => _linkUp;

    /// <summary>True once the kit delivered the link, unless it was disabled since.</summary>
    public override bool Ready => _state == LinkState.Live && _enabled;

    /// <summary>Creates the link <paramref name="context"/>'s driver publishes. Thread context.</summary>
    /// <param name="context">The binding attempt that published it, which names it and its log lines.</param>
    /// <param name="address">The MAC address the stack sends from.</param>
    /// <param name="transmit">The driver's transmit path.</param>
    internal PublishedNetworkDevice(DeviceContext context, MACAddress address, NetworkTransmitHandler transmit)
    {
        _context = context;
        MacAddress = address;
        _transmit = transmit;
        Name = $"{context.DriverName} {context.Path}";
    }

    /// <summary>
    /// Makes a queued link live, from which point it carries traffic. The
    /// kit calls it as it delivers the link to the network manager, right
    /// after the driver's Probe returned Bound. A link dropped with its
    /// attempt stays dropped.
    /// </summary>
    public override void Initialize()
    {
        if (_state == LinkState.Queued)
        {
            _state = LinkState.Live;
        }
    }

    /// <summary>Lets the link carry traffic again after <see cref="Disable"/>.</summary>
    public override void Enable() => _enabled = true;

    /// <summary>Stops the link carrying traffic: sends fail and received frames are dropped, until <see cref="Enable"/>.</summary>
    public override void Disable() => _enabled = false;

    /// <summary>
    /// Hands one frame to the driver's transmit handler, with interrupts
    /// masked and one call at a time. An exception from the handler is
    /// logged with the driver's name and the device's path, and counts as a
    /// failed send, so a faulty driver cannot unwind the stack.
    /// </summary>
    /// <param name="data">The frame.</param>
    /// <param name="length">How many bytes of <paramref name="data"/> the frame is.</param>
    /// <returns>False when the link does not carry traffic, the length is out of range, or the driver refused the frame.</returns>
    public override bool Send(byte[] data, int length)
    {
        // First, and cheap: a link that is not live never reaches the driver.
        if (_state != LinkState.Live || !_enabled)
        {
            return false;
        }

        if ((uint)length > (uint)data.Length)
        {
            return false;
        }

        try
        {
            using (_transmitLock.AcquireIrqSafe())
            {
                return _transmit(new ReadOnlySpan<byte>(data, 0, length));
            }
        }
        catch (Exception exception)
        {
            _context.WriteLog($"the transmit handler threw: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// Hands a received frame to the stack, as <see cref="NetworkLink.Deliver"/>
    /// describes. The copy is a new array each time and never a pooled one:
    /// the stack keeps the arrays it is handed, such as a UDP datagram
    /// waiting in a client's receive queue.
    /// </summary>
    internal void Deliver(ReadOnlySpan<byte> frame)
    {
        // Checked before the copy, so a link nobody listens on allocates nothing.
        if (_state != LinkState.Live || !_enabled || frame.IsEmpty || OnPacketReceived is null)
        {
            return;
        }

        byte[] copy = frame.ToArray();

        // The built-in drivers run the stack's receive path from their
        // interrupt handlers, and the stack relies on it: it takes no lock of
        // its own. Masking interrupts here gives a frame delivered from a
        // thread the same footing, so no other frame, and no timer callback,
        // runs the stack halfway through this one.
        using (InternalCpu.DisableInterruptsScope())
        {
            OnPacketReceived?.Invoke(copy, copy.Length);
        }
    }

    /// <summary>Records the link state the driver reports. IRQ-safe.</summary>
    internal void SetLinkState(bool isUp) => _linkUp = isUp;

    /// <summary>
    /// Drops a queued link with the attempt that published it: it never
    /// reaches the network manager, and nothing sent or delivered through it
    /// goes anywhere.
    /// </summary>
    internal void Drop() => _state = LinkState.Gone;
}
