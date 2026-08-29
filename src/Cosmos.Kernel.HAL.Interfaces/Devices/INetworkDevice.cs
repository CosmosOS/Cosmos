// This code is licensed under the BSD 3-Clause license (see LICENSE for details)


namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Delegate for handling packet received events.
/// </summary>
/// <param name="data">The received packet data.</param>
/// <param name="length">The length of the packet.</param>
internal delegate void PacketReceivedHandler(byte[] data, int length);

/// <summary>
/// Interface for network devices. Internal: a kernel neither obtains one nor
/// supplies one, it goes through <c>NetworkManager</c> like every other
/// device manager in the ring.
/// </summary>
internal interface INetworkDevice
{
    /// <summary>
    /// Initialize the network device.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Gets the MAC address of the device.
    /// </summary>
    MACAddress MacAddress { get; }

    /// <summary>
    /// Gets whether the link is up.
    /// </summary>
    bool LinkUp { get; }

    /// <summary>
    /// Gets whether the device is ready to send/receive.
    /// </summary>
    bool Ready { get; }

    /// <summary>
    /// Sends a packet over the network.
    /// </summary>
    /// <param name="data">The packet data to send.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>True if the packet was queued successfully.</returns>
    bool Send(byte[] data, int length);

    /// <summary>
    /// Event handler for packet received events.
    /// </summary>
    PacketReceivedHandler? OnPacketReceived { get; set; }

    /// <summary>
    /// Enable the network device.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disable the network device.
    /// </summary>
    void Disable();

    /// <summary>
    /// Gets the device name.
    /// </summary>
    string Name { get; }
}
