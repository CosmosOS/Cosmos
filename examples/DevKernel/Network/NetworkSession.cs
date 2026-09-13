using System;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.UDP;

namespace DevKernel.Network;

/// <summary>
/// The IPv4 configuration the shell applied to the primary NIC, plus the UDP
/// receive callback that dumps incoming payloads to serial and console. One
/// instance lives for the whole shell session.
/// </summary>
internal sealed class NetworkSession
{
    /// <summary>First octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet1 = 10;

    /// <summary>Second octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet2 = 0;

    /// <summary>Third octet of the QEMU user-networking (SLIRP) 10.0.2.0/24 subnet.</summary>
    private const byte QemuNetOctet3 = 2;

    /// <summary>Host octet of the default QEMU guest IP (10.0.2.15).</summary>
    private const byte QemuGuestHostOctet = 15;

    /// <summary>Host octet of the QEMU user-networking gateway IP (10.0.2.2).</summary>
    private const byte QemuGatewayHostOctet = 2;

    /// <summary>Fully-masked octet of the /24 subnet mask (255.255.255.0).</summary>
    private const byte SubnetMaskFullOctet = 255;

    /// <summary>Unmasked host octet of the /24 subnet mask (255.255.255.0).</summary>
    private const byte SubnetMaskHostOctet = 0;

    /// <summary>Maximum UDP payload bytes echoed to the console per packet.</summary>
    private const int UdpPreviewMaxBytes = 64;

    /// <summary>Address assigned to this machine, once configured.</summary>
    public Address? LocalIp { get; private set; }

    /// <summary>Default gateway, once configured.</summary>
    public Address? GatewayIp { get; private set; }

    /// <summary>True once either <see cref="ConfigureStatic"/> or <see cref="AdoptLease"/> has run.</summary>
    public bool IsConfigured { get; private set; }

    /// <summary>
    /// Brings up the stack with the static QEMU user-networking address plan.
    /// The subnet and gateway are passed through so <c>IPConfig.FindNetwork()</c>
    /// can route outbound packets.
    /// </summary>
    public void ConfigureStatic(INetworkDevice device)
    {
        LocalIp = new Address4(QemuNetOctet1, QemuNetOctet2, QemuNetOctet3, QemuGuestHostOctet);
        GatewayIp = new Address4(QemuNetOctet1, QemuNetOctet2, QemuNetOctet3, QemuGatewayHostOctet);
        Address4 subnet = new(SubnetMaskFullOctet, SubnetMaskFullOctet, SubnetMaskFullOctet, SubnetMaskHostOctet);

        NetworkStack.Initialize();
        IPConfig.Enable(device, LocalIp, subnet, GatewayIp);

        AttachUdpListener();
        IsConfigured = true;
    }

    /// <summary>Records the addresses a DHCP server handed out and starts listening for UDP.</summary>
    public void AdoptLease(Address localIp, Address gatewayIp)
    {
        LocalIp = localIp;
        GatewayIp = gatewayIp;

        AttachUdpListener();
        IsConfigured = true;
    }

    private void AttachUdpListener()
    {
        UDPPacket.OnUDPDataReceived = OnUdpDataReceived;
    }

    /// <summary>Logs the full payload to serial, and a printable preview to the console.</summary>
    private void OnUdpDataReceived(UDPPacket packet)
    {
        Serial.Write("[UDP] Received packet from ");
        Serial.WriteString(packet.SourceIP.ToString());
        Serial.Write(":");
        Serial.WriteNumber((ulong)packet.SourcePort);
        Serial.Write(" -> port ");
        Serial.WriteNumber((ulong)packet.DestinationPort);
        Serial.Write("\n");

        byte[] data = packet.UDPData;
        Serial.Write("[UDP] Payload (");
        Serial.WriteNumber((ulong)data.Length);
        Serial.Write(" bytes): ");

        for (int i = 0; i < data.Length; i++)
        {
            char c = (char)data[i];
            if (Ascii.IsPrintable(c))
            {
                Serial.Write(c.ToString());
            }
        }

        Serial.Write("\n");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("[UDP] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(packet.SourceIP.ToString() + ":" + packet.SourcePort.ToString());
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" -> ");
        Console.ResetColor();

        for (int i = 0; i < data.Length && i < UdpPreviewMaxBytes; i++)
        {
            char c = (char)data[i];
            if (Ascii.IsPrintable(c))
            {
                Console.Write(c.ToString());
            }
        }

        Console.WriteLine();
    }
}
