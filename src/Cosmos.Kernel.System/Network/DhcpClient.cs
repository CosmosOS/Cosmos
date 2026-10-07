// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Client
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network.Protocols.IPv4.Dhcp;
using Cosmos.Kernel.System.Timers;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Used to manage the DHCP connection to a server.
/// </summary>
public sealed class DhcpClient : UdpClient
{
    private const byte BootReplyOperation = 2;

    // The message type (option 53) is read at a fixed frame offset, on the
    // assumption that the server sends it as the first option.
    private const int MessageTypeOffset = 284;

    private const byte OfferMessageType = 0x02;
    private const byte AckMessageType = 0x05;
    private const byte NakMessageType = 0x06;

    // Set once an ACK's configuration is in force; SendDiscoverPacket clears it.
    private bool _applied;

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpClient"/> class.
    /// </summary>
    public DhcpClient() : base(68)
    {
    }

    /// <summary>
    /// Gets the address of the DHCP server for a device, taken to be its default
    /// gateway, or null when the device has no IP configuration.
    /// </summary>
    internal static Address? DHCPServerAddress(INetworkDevice networkDevice) => IPConfig.Get(networkDevice)?.DefaultGateway;

    /// <summary>
    /// Waits for the server's reply and acts on it: an offer is answered with a
    /// request, whose reply is awaited in turn, and an ACK or a NAK is applied.
    /// </summary>
    /// <param name="timeout">How long to wait, in milliseconds.</param>
    /// <returns>The milliseconds waited for the last reply, or -1 when none arrived in time.</returns>
    private int Receive(int timeout = 5000)
    {
        int waited = 0;

        while (_rxBuffer.Count < 1 && waited < timeout)
        {
            TimerManager.Wait(100);
            waited += 100;
        }

        if (_rxBuffer.Count < 1)
        {
            return -1;
        }

        DhcpPacket packet = new(_rxBuffer.Dequeue().RawData);

        if (packet.Operation == BootReplyOperation)
        {
            byte messageType = packet.RawData[MessageTypeOffset];
            if (messageType == OfferMessageType)
            {
                Serial.WriteString("[DHCP] Offer received.\n");
                return SendRequestPacket(packet.Client ?? throw new Exception($"{nameof(packet.Client)} can not be null"));
            }
            else if (messageType == AckMessageType || messageType == NakMessageType)
            {
                if (!_applied)
                {
                    Apply(packet);

                    Close();
                }
            }
        }

        return waited;
    }

    /// <summary>
    /// Sends a packet to the DHCP server in order to make the address available again.
    /// </summary>
    /// <exception cref="Exception">A registered device has no IP
    /// configuration to take the server address from, or no configured interface
    /// can reach that address.</exception>
    public void SendReleasePacket()
    {
        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            INetworkDevice? networkDevice = NetworkManager.GetDevice(i);
            if (networkDevice is null)
            {
                continue;
            }

            Address destIp = DHCPServerAddress(networkDevice) ?? throw new Exception("IP can not be null");
            Address source = IPConfig.FindNetwork(destIp)
                ?? throw new Exception("Address can not be null");
            DhcpRelease dhcpRelease = new(source, destIp, networkDevice.MacAddress);

            dhcpRelease.Network.Enqueue();
            NetworkStack.Update();

            NetworkStack.RemoveAllConfigIP();

            IPConfig.Enable(networkDevice, new Address4(0, 0, 0, 0), new Address4(0, 0, 0, 0), new Address4(0, 0, 0, 0));
        }

        Close();
    }

    /// <summary>
    /// Send a packet to find the DHCP server and inform the host that we
    /// are requesting a new IP address.
    /// </summary>
    /// <returns>The amount of time elapsed, or -1 if a timeout has been reached.</returns>
    /// <exception cref="Exception">The server answered with an offer,
    /// an ACK or a NAK that carries no client address.</exception>
    public int SendDiscoverPacket()
    {
        NetworkStack.RemoveAllConfigIP();

        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            INetworkDevice? networkDevice = NetworkManager.GetDevice(i);
            if (networkDevice is null)
            {
                continue;
            }

            IPConfig.Enable(networkDevice, new Address4(0, 0, 0, 0), new Address4(0, 0, 0, 0), new Address4(0, 0, 0, 0));

            DhcpDiscover dhcpDiscover = new(networkDevice.MacAddress);
            dhcpDiscover.Network.Enqueue();
            NetworkStack.Update();

            _applied = false;
        }

        return Receive();
    }

    /// <summary>
    /// Sends a request to apply the new IP configuration.
    /// </summary>
    /// <returns>The amount of time elapsed, or -1 if a timeout has been reached.</returns>
    private int SendRequestPacket(Address requestedAddress)
    {
        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            INetworkDevice? networkDevice = NetworkManager.GetDevice(i);
            if (networkDevice is null)
            {
                continue;
            }

            DhcpRequest dhcpRequest = new(networkDevice.MacAddress, requestedAddress);
            dhcpRequest.Network.Enqueue();
            NetworkStack.Update();
        }
        return Receive();
    }

    /// <summary>
    /// Applies the newly received IP configuration.
    /// </summary>
    /// <param name="packet">The DHCP ACK packet.</param>
    private void Apply(DhcpPacket packet)
    {
        if (!_applied)
        {
            NetworkStack.RemoveAllConfigIP();

            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                INetworkDevice? networkDevice = NetworkManager.GetDevice(i);
                if (networkDevice is null)
                {
                    continue;
                }

                if (packet.Client is null || packet.Client.ToString() is null)
                {
                    throw new Exception("Parsing DHCP ACK Packet failed, can't apply network configuration.");
                }
                else
                {
                    Serial.WriteString("[DHCP ACK] Packet received, applying IP configuration...\n");
                    Serial.WriteString($"   IP Address  : {packet.Client}\n");
                    Serial.WriteString($"   Subnet mask : {packet.Subnet?.ToString() ?? "null"}\n");
                    Serial.WriteString($"   Gateway     : {packet.Gateway?.ToString() ?? "null"}\n");
                    Serial.WriteString($"   DNS server  : {packet.DNS?.ToString() ?? "null"}\n");

                    IPConfig.Enable(networkDevice, packet.Client, packet.Subnet ?? new Address4(255, 255, 255, 0), packet.Gateway ?? Address4.Zero);
                    if (packet.DNS is not null)
                    {
                        DnsConfig.Add(packet.DNS);
                    }

                    Serial.WriteString("[DHCP CONFIG] IP configuration _applied.\n");

                    _applied = true;

                    return;
                }
            }

            Serial.WriteString("[DHCP CONFIG] No DHCP Config _applied!\n");
        }
        else
        {
            Serial.WriteString("[DHCP CONFIG] DHCP already _applied.\n");
        }
    }
}
