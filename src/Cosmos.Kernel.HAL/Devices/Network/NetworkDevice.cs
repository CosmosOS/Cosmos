// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Network;

/// <summary>
/// Abstract base class for all network devices.
/// </summary>
internal abstract class NetworkDevice : Device, INetworkDevice
{
    /// <summary>
    /// Event handler for packet received events.
    /// </summary>
    public PacketReceivedHandler? OnPacketReceived { get; set; }

    /// <summary>
    /// Initialize the network device.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Gets the MAC address of the device.
    /// </summary>
    public abstract MACAddress MacAddress { get; }

    /// <summary>
    /// Gets whether the link is up.
    /// </summary>
    public abstract bool LinkUp { get; }

    /// <summary>
    /// Gets whether the device is ready to send/receive.
    /// </summary>
    public abstract bool Ready { get; }

    /// <summary>
    /// Sends a packet over the network.
    /// </summary>
    /// <param name="data">The packet data to send.</param>
    /// <param name="length">The length of the packet.</param>
    /// <returns>True if the packet was queued successfully.</returns>
    public abstract bool Send(byte[] data, int length);

    /// <summary>
    /// Enable the network device.
    /// </summary>
    public abstract void Enable();

    /// <summary>
    /// Disable the network device.
    /// </summary>
    public abstract void Disable();

    /// <summary>
    /// Gets the device name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Formats a MAC address as a string.
    /// </summary>
    protected static string FormatMacAddress(byte[] mac)
    {
        if (mac == null || mac.Length != 6)
        {
            return "00:00:00:00:00:00";
        }

        return $"{mac[0]:X2}:{mac[1]:X2}:{mac[2]:X2}:{mac[3]:X2}:{mac[4]:X2}:{mac[5]:X2}";
    }
}
