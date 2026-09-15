/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Client
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.UDP;
using Cosmos.Kernel.System.Timer;

namespace Cosmos.Kernel.System.Network.IPv4.DHCP;

/// <summary>
/// Used to manage the DHCP connection to a server.
/// </summary>
public sealed class DhcpClient : UdpClient
{
    /// <summary>
    /// Is DHCP asked check variable
    /// </summary>
    private bool _applied = false;

    /// <summary>
    /// Gets the IP address of the DHCP server.
    /// </summary>
    internal static Address? DHCPServerAddress(INetworkDevice networkDevice)
    {
        return IPConfig.Get(networkDevice)?.DefaultGateway;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DhcpClient"/> class.
    /// </summary>
    public DhcpClient() : base(68)
    {
    }

    /// <summary>
    /// Receive data
    /// </summary>
    /// <param name="timeout">timeout value, default 5000ms</param>
    /// <returns>time value (-1 = timeout)</returns>
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

        if (packet.Operation == 2) //Boot Reply
        {
            if (packet.RawData[284] == 0x02) //Offer packet received
            {
                Serial.WriteString("[DHCP] Offer received.\n");
                return SendRequestPacket(packet.Client ?? throw new Exception($"{nameof(packet.Client)} can not be null"));
            }
            else if (packet.RawData[284] == 0x05 || packet.RawData[284] == 0x06) //ACK or NAK DHCP packet received
            {
                if (!_applied)
                {
                    Apply(packet, true);

                    Close();
                }
            }
        }

        return waited;
    }

    /// <summary>
    /// Sends a packet to the DHCP server in order to make the address available again.
    /// </summary>
    public void SendReleasePacket()
    {
        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            var networkDevice = NetworkManager.GetDevice(i);
            if (networkDevice is null)
            {
                continue;
            }

            var destIp = DHCPServerAddress(networkDevice) ?? throw new Exception($"IP can not be null");
            Address source = IPConfig.FindNetwork(destIp)
                ?? throw new Exception($"Address can not be null");
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
    public int SendDiscoverPacket()
    {
        NetworkStack.RemoveAllConfigIP();

        for (int i = 0; i < NetworkManager.DeviceCount; i++)
        {
            var networkDevice = NetworkManager.GetDevice(i);
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
            var networkDevice = NetworkManager.GetDevice(i);
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
    /// <param name="message">Enable/Disable the displaying of messages about DHCP applying and conf.</param>
    private void Apply(DhcpPacket packet, bool message = false)
    {
        if (!_applied)
        {
            NetworkStack.RemoveAllConfigIP();

            for (int i = 0; i < NetworkManager.DeviceCount; i++)
            {
                var networkDevice = NetworkManager.GetDevice(i);
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
