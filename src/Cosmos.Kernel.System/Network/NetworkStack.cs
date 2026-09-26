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
    internal static Dictionary<Address, INetworkDevice> AddressMap { get; } = [];

    /// <summary>
    /// Maps MAC addresses to network devices.
    /// </summary>
    internal static Dictionary<uint, INetworkDevice> MACMap { get; } = [];

    /// <summary>
    /// Configures an IP address on the given network device, together with
    /// the link-local IPv6 address derived from the device's MAC address.
    /// </summary>
    /// <param name="device">The target network device.</param>
    /// <param name="ipAddress">The IP address to assign to the device.</param>
    internal static void ConfigIP(INetworkDevice device, Address ipAddress)
    {
        var mac = device.MacAddress;

        // Remove old config if exists
        if (MACMap.ContainsKey(mac.Hash))
        {
            RemoveAddresses(device);
            MACMap.Remove(mac.Hash);
        }

        // Add new config. The link-local IPv6 address needs nothing from the
        // caller: it is derived from the MAC, so it comes up with the first
        // configuration.
        AddressMap.Add(ipAddress, device);
        AddressMap[IPv6.Address6.LinkLocalFor(mac)] = device;
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
    /// <see cref="Config.IPConfig.Enable(Address, Address, Address)"/>,
    /// which is the public form of this and always was.
    /// </remarks>
    internal static void ConfigIP(INetworkDevice device, IPConfig config)
    {
        ConfigIP(device, config.Address);
        IPConfig.Set(device, config);
    }

    /// <summary>
    /// Forgets every address mapped to a device.
    /// </summary>
    /// <param name="device">The device being reconfigured or taken out.</param>
    private static void RemoveAddresses(INetworkDevice device)
    {
        List<Address> stale = [];
        foreach (KeyValuePair<Address, INetworkDevice> pair in AddressMap)
        {
            if (pair.Value == device)
            {
                stale.Add(pair.Key);
            }
        }

        foreach (Address address in stale)
        {
            AddressMap.Remove(address);
        }
    }

    /// <summary>
    /// Forgets a device that left the network manager: every address mapped
    /// to it, its MAC address and its IPv4 configuration, and it no longer
    /// hands the stack what it receives. The network manager's
    /// unregistration calls it, with interrupts off, so no receive path runs
    /// the stack halfway through.
    /// </summary>
    /// <param name="device">The device that left.</param>
    internal static void RemoveDevice(INetworkDevice device)
    {
        RemoveAddresses(device);

        // Found by the device each entry maps to, as for the addresses above,
        // rather than by the key its MAC address gives now.
        List<uint> staleMacs = [];
        foreach (KeyValuePair<uint, INetworkDevice> pair in MACMap)
        {
            if (pair.Value == device)
            {
                staleMacs.Add(pair.Key);
            }
        }

        foreach (uint mac in staleMacs)
        {
            MACMap.Remove(mac);
        }

        IPConfig.Remove(device);
        device.OnPacketReceived = null;
    }

    /// <summary>
    /// The link-local IPv6 address mapped to a device, or null while the
    /// device is unconfigured.
    /// </summary>
    /// <param name="device">The device to look up.</param>
    internal static IPv6.Address6? LinkLocalOf(INetworkDevice device)
    {
        foreach (KeyValuePair<Address, INetworkDevice> pair in AddressMap)
        {
            if (pair.Value == device && pair.Key is IPv6.Address6 address6)
            {
                return address6;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes all IP configurations, clearing the stack's address and MAC
    /// maps with them. The counterpart of <see cref="Config.IPConfig.Enable(Address, Address, Address)"/>.
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
        IPv6.OutgoingBuffer.Send();
        s_updating = false;
    }

    /// <summary>
    /// Transmits a packet through the stack's outgoing queue: the sending
    /// device is resolved from the packet's source address, the destination
    /// MAC address is resolved by ARP or by Neighbor Discovery depending on
    /// the packet's version, and the queue is pumped before returning.
    /// </summary>
    /// <param name="packet">A built packet, typically created through one of the
    /// packet type constructors. A transport packet passes its
    /// <c>Network</c> property here.</param>
    /// <returns>False when no configured interface matches the packet's source address; the packet is not queued in that case.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public static bool Send(InternetPacket packet)
    {
        if (!packet.Enqueue())
        {
            return false;
        }

        Update();
        return true;
    }

    /// <summary>
    /// Injects a received Ethernet frame into the stack: the frame is
    /// dispatched to the ARP, IPv4 or IPv6 handler by EtherType, exactly as a
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
            case 0x86DD: // IPv6
                Serial.WriteString("[NetworkStack] -> IPv6\n");
                IPv6.IPv6Packet.IPv6Handler(packetData);
                break;
            default:
                Serial.WriteString("[NetworkStack] Unknown EtherType, ignoring\n");
                break;
        }
    }
}
