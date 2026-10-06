// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The ring's view of a network interface a driver kit driver published:
/// an <see cref="INetworkDevice"/> over the <see cref="INetworkInterface"/>,
/// so the stack, its configuration and its clients keep their one contract
/// over the kit's devices. Created by <see cref="KitNetworkConsumer"/>
/// on the kit worker when the interface is published; frames reach
/// <see cref="OnPacketReceived"/> through the consumer, and link changes
/// land in <see cref="LinkUp"/>. The reads are any context.
/// </summary>
internal sealed class KitNetworkDevice : INetworkDevice
{
    private readonly PublishedDevice _published;
    private readonly INetworkInterface _network;
    private volatile bool _linkUp;

    /// <summary>Wraps a published interface; the link state starts as the interface reports it. Kit worker, thread context.</summary>
    /// <param name="published">The published device the interface came with.</param>
    /// <param name="network">The driver's interface contract.</param>
    internal KitNetworkDevice(PublishedDevice published, INetworkInterface network)
    {
        _published = published;
        _network = network;
        _linkUp = network.LinkUp;
    }

    /// <summary>The published device this adapter stands for, the key the consumer finds it by.</summary>
    public PublishedDevice Published => _published;

    /// <inheritdoc/>
    public string Name => _network.Name;

    /// <inheritdoc/>
    public MACAddress MacAddress => _network.MacAddress;

    /// <summary>True while the link is up, as the driver last reported it through its sink; false once the device is withdrawn.</summary>
    public bool LinkUp
    {
        get => _linkUp && !_published.IsWithdrawn;
        internal set => _linkUp = value;
    }

    /// <summary>Always true: a kit driver publishes an interface only once it carries traffic.</summary>
    public bool Ready => true;

    /// <inheritdoc/>
    public PacketReceivedHandler? OnPacketReceived { get; set; }

    /// <summary>Nothing to do: the kit driver brought the device up in its probe, before publishing it.</summary>
    public void Initialize()
    {
    }

    /// <summary>Nothing to do: a published interface is always enabled, and the driver's teardown withdraws it.</summary>
    public void Enable()
    {
    }

    /// <summary>Nothing to do: see <see cref="Enable"/>.</summary>
    public void Disable()
    {
    }

    /// <summary>
    /// Hands the first <paramref name="length"/> bytes of <paramref name="data"/>
    /// to the interface; nothing once the device is withdrawn, since the
    /// contract object must not be used after that (the stack's configuration
    /// still names the device then). Any context the interface's transmit
    /// path allows.
    /// </summary>
    /// <param name="data">The frame.</param>
    /// <param name="length">How many bytes of it to send.</param>
    /// <returns>False when the device is withdrawn, the arguments are unusable or the interface did not take the frame.</returns>
    public bool Send(byte[] data, int length) =>
        !_published.IsWithdrawn && data is not null && length > 0 && length <= data.Length && _network.Transmit(data.AsSpan(0, length));
}
