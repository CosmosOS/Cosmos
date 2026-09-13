using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.ARP;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Manages the Cosmos networking stack.
/// </summary>
public static class NetworkStack
{
    /// <summary>Reentrancy guard for <see cref="Update"/>.</summary>
    private static bool s_updating = false;

    /// <summary>
    /// Maps IP (Internet Protocol) addresses to network devices.
    /// </summary>
    internal static Dictionary<uint, INetworkDevice> AddressMap { get; } = [];

    /// <summary>
    /// Maps MAC addresses to network devices.
    /// </summary>
    internal static Dictionary<uint, INetworkDevice> MACMap { get; } = [];

    /// <summary>
    /// Configures an IP address on the given network device.
    /// </summary>
    /// <param name="device">The target network device.</param>
    /// <param name="ipAddress">The IP address to assign to the device.</param>
    internal static void ConfigIP(INetworkDevice device, Address ipAddress)
    {
        var mac = device.MacAddress;

        // Remove old config if exists
        if (MACMap.ContainsKey(mac.Hash))
        {
            // Find and remove old IP mapping
            foreach (KeyValuePair<uint, INetworkDevice> pair in AddressMap)
            {
                if (pair.Value == device)
                {
                    AddressMap.Remove(pair.Key);
                    break;
                }
            }
            MACMap.Remove(mac.Hash);
        }

        // Add new config
        AddressMap.Add(ipAddress.Id, device);
        MACMap.Add(mac.Hash, device);

        // Register packet handler
        device.OnPacketReceived = HandlePacket;

        Serial.WriteString("[NetworkStack] Configured IP ");
        Serial.WriteString(ipAddress.ToString());
        Serial.WriteString(" on device ");
        Serial.WriteString(device.Name);
        Serial.WriteString("\n");
    }

    /// <summary>
    /// Configures an IP address on the given network device using IPConfig.
    /// </summary>
    /// <param name="device">The target network device.</param>
    /// <param name="config">The IP configuration to apply.</param>
    /// <remarks>
    /// Internal: a kernel configures the primary device through
    /// <see cref="Config.IPConfig.Enable(IPv4.Address, IPv4.Address, IPv4.Address)"/>,
    /// which is the public form of this and always was.
    /// </remarks>
    internal static void ConfigIP(INetworkDevice device, IPConfig config)
    {
        ConfigIP(device, config.Address);
        IPConfig.Set(device, config);
    }

    /// <summary>
    /// Removes all IP configurations, clearing the stack's address and MAC
    /// maps with them. The counterpart of <see cref="Config.IPConfig.Enable(IPv4.Address, IPv4.Address, IPv4.Address)"/>.
    /// </summary>
    public static void RemoveAllConfigIP()
    {
        AddressMap.Clear();
        MACMap.Clear();
        IPConfig.RemoveAll();
    }

    /// <summary>
    /// Flag to prevent recursive Update calls.
    /// </summary>
    /// <summary>
    /// Updates the network stack (sends pending packets). Internal: every
    /// path that queues a packet pumps the queue itself, including
    /// <see cref="Send"/> and each protocol client.
    /// </summary>
    internal static void Update()
    {
        // Prevent recursive calls
        if (s_updating)
        {
            return;
        }

        s_updating = true;
        OutgoingBuffer.Send();
        s_updating = false;
    }

    /// <summary>
    /// Transmits a packet through the stack's outgoing queue: the sending
    /// device is resolved from the packet's source address, the destination
    /// MAC is resolved by ARP for non-broadcast destinations, and the queue
    /// is pumped before returning.
    /// </summary>
    /// <param name="packet">A built packet, typically created through one of the packet type constructors.</param>
    /// <returns>False when no configured interface matches the packet's source address; the packet is not queued in that case.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public static bool Send(IPPacket packet)
    {
        if (!OutgoingBuffer.AddPacket(packet))
        {
            return false;
        }

        Update();
        return true;
    }

    /// <summary>
    /// Injects a received Ethernet frame into the stack: the frame is
    /// dispatched to the ARP or IPv4 handler by EtherType, exactly as a
    /// frame arriving from a network device would be. This is the receive
    /// entry point registered on every configured device.
    /// </summary>
    /// <param name="packetData">Packet data array.</param>
    /// <param name="length">Packet length.</param>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public static void HandlePacket(byte[] packetData, int length)
    {
        Serial.WriteString("[NetworkStack] HandlePacket called, len=");
        Serial.WriteNumber((ulong)length);
        Serial.WriteString("\n");

        if (length < 14)
        {
            Serial.WriteString("[NetworkStack] Error: Invalid packet data\n");
            return;
        }

        ushort etherType = (ushort)((packetData[12] << 8) | packetData[13]);
        Serial.WriteString("[NetworkStack] EtherType: 0x");
        Serial.WriteHex(etherType);
        Serial.WriteString("\n");

        switch (etherType)
        {
            case 0x0806: // ARP
                Serial.WriteString("[NetworkStack] -> ARP\n");
                ArpPacket.ARPHandler(packetData);
                break;
            case 0x0800: // IPv4
                Serial.WriteString("[NetworkStack] -> IPv4\n");
                IPPacket.IPv4Handler(packetData);
                break;
            default:
                Serial.WriteString("[NetworkStack] Unknown EtherType, ignoring\n");
                break;
        }
    }
}
