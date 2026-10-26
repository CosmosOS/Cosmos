// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DHCP Client
* PROGRAMMERS:      Alexy DA CRUZ <dacruzalexy@gmail.com>
*                   Valentin CHARBONNIER <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network.Protocols.IPv4;
using Cosmos.Kernel.System.Network.Protocols.IPv4.Dhcp;
using Cosmos.Kernel.System.Timers;

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Obtains an IPv4 configuration for one network adapter from a DHCP server:
/// the primary adapter by default, or the one handed to the constructor. The
/// exchange leaves through that adapter alone and only that adapter's
/// configuration is touched; every other adapter keeps its own.
/// </summary>
public sealed class DhcpClient : UdpClient
{
    private const byte BootReplyOperation = 2;

    // The message type (option 53) is read at a fixed frame offset, on the
    // assumption that the server sends it as the first option.
    private const int MessageTypeOffset = 284;

    // chaddr, the client hardware address a server copies back from the
    // request it answers: what tells this adapter's replies from another's.
    private const int ClientHardwareAddressOffset = 70;

    private const byte OfferMessageType = 0x02;
    private const byte AckMessageType = 0x05;
    private const byte NakMessageType = 0x06;

    /// <summary>What <see cref="SendDiscoverPacket"/> returns when the exchange ends without a lease.</summary>
    private const int NoLease = -1;

    // Null for a client of the primary adapter, whichever it is when an
    // exchange runs: a kernel may build its client before the drivers bind.
    private readonly NetworkAdapter? _adapter;

    /// <summary>
    /// Initializes a client for the primary adapter, <see cref="NetworkManager.Primary"/>
    /// as of each exchange.
    /// </summary>
    public DhcpClient() : base(68)
    {
    }

    /// <summary>
    /// Initializes a client for one adapter. The handle is resolved when an
    /// exchange runs, so a device withdrawn in between is reported then.
    /// </summary>
    /// <param name="adapter">Handle to the adapter to configure, from <see cref="NetworkManager.GetAdapter(int)"/>.</param>
    public DhcpClient(NetworkAdapter adapter) : base(68)
    {
        _adapter = adapter;
    }

    /// <summary>
    /// The device the client configures.
    /// </summary>
    /// <exception cref="InvalidOperationException">The adapter names no registered device.</exception>
    private INetworkDevice RequireDevice()
    {
        NetworkAdapter adapter = _adapter ?? NetworkManager.Primary;
        return adapter.Device ?? throw new InvalidOperationException("The adapter names no registered network device.");
    }

    /// <summary>
    /// Waits for the server's reply to this adapter's exchange and acts on it:
    /// an offer is answered with a request, whose reply is awaited in turn, an
    /// ACK is applied, and a NAK ends the exchange with no lease. A reply that
    /// carries another hardware address belongs to another adapter's exchange
    /// and is skipped.
    /// </summary>
    /// <param name="device">The device the exchange runs on.</param>
    /// <param name="timeout">How long to wait, in milliseconds.</param>
    /// <returns>The milliseconds waited for the last reply, or -1 when none arrived in time or the server declined.</returns>
    private int Receive(INetworkDevice device, int timeout = 5000)
    {
        int waited = 0;

        while (waited < timeout)
        {
            if (_rxBuffer.Count < 1)
            {
                TimerManager.Wait(100);
                waited += 100;
                continue;
            }

            DhcpPacket packet = new(_rxBuffer.Dequeue().RawData);
            if (packet.Operation != BootReplyOperation || !IsAddressedTo(packet, device))
            {
                continue;
            }

            byte messageType = packet.RawData[MessageTypeOffset];
            if (messageType == OfferMessageType)
            {
                Serial.WriteString("[DHCP] Offer received.\n");
                return SendRequestPacket(device, packet.Client ?? throw new InvalidOperationException($"{nameof(packet.Client)} can not be null"));
            }

            if (messageType == AckMessageType)
            {
                Apply(device, packet);
                Close();
                return waited;
            }

            if (messageType == NakMessageType)
            {
                Serial.WriteString("[DHCP] Request declined by the server (NAK), no lease.\n");
                Close();
                return NoLease;
            }
        }

        // No reply in time: stop receiving on the client port, so the frames
        // that keep arriving are not queued for nobody.
        Close();
        return NoLease;
    }

    /// <summary>
    /// Whether a reply answers this device's request: the server copies the
    /// client hardware address of the request it answers.
    /// </summary>
    private static bool IsAddressedTo(DhcpPacket packet, INetworkDevice device)
    {
        byte[] mac = device.MacAddress._bytes;
        for (int i = 0; i < mac.Length; i++)
        {
            if (packet.RawData[ClientHardwareAddressOffset + i] != mac[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Tells the server the adapter's address is free again and drops the
    /// adapter's IP configuration, leaving it unconfigured. The server is taken
    /// to be the adapter's default gateway. Other adapters are left alone.
    /// </summary>
    /// <exception cref="InvalidOperationException">The adapter names no
    /// registered device, or has no IP configuration to release.</exception>
    public void SendReleasePacket()
    {
        INetworkDevice device = RequireDevice();
        IPConfig config = IPConfig.Get(device) ?? throw new InvalidOperationException("The adapter has no IP configuration to release.");

        DhcpRelease dhcpRelease = new(config.Address, config.DefaultGateway, device.MacAddress);
        Transmit(dhcpRelease, device);

        NetworkStack.RemoveConfigIP(device);
        Close();
    }

    /// <summary>
    /// Runs the DISCOVER, OFFER, REQUEST, ACK exchange on the adapter and
    /// applies the lease to it. The adapter's configuration is replaced when
    /// the server acknowledges and left as it was otherwise; no other adapter
    /// is touched.
    /// </summary>
    /// <returns>The milliseconds waited, or -1 when no lease was obtained:
    /// no reply arrived in time, or the server declined with a NAK.</returns>
    /// <exception cref="InvalidOperationException">The adapter names no
    /// registered device, or the server answered with an offer or an ACK that
    /// carries no client address.</exception>
    public int SendDiscoverPacket()
    {
        INetworkDevice device = RequireDevice();

        // The stack hands a device's frames up only once it listens on it, and
        // a device with no address yet has to hear the offer all the same.
        NetworkStack.Listen(device);

        DhcpDiscover dhcpDiscover = new(device.MacAddress);
        Transmit(dhcpDiscover, device);

        return Receive(device);
    }

    /// <summary>
    /// Sends a request for the offered address and waits for the server's answer.
    /// </summary>
    /// <returns>The amount of time elapsed, or -1 if a timeout has been reached.</returns>
    private int SendRequestPacket(INetworkDevice device, Address requestedAddress)
    {
        DhcpRequest dhcpRequest = new(device.MacAddress, requestedAddress);
        Transmit(dhcpRequest, device);

        return Receive(device);
    }

    /// <summary>
    /// Queues a packet on the exchange's own device and pumps the queue. The
    /// stack would otherwise pick the device from the packet's source address,
    /// and a client with no lease yet sends from 0.0.0.0, which names none.
    /// </summary>
    private static void Transmit(DhcpPacket packet, INetworkDevice device)
    {
        IPv4OutgoingBuffer.AddPacket((IPPacket)packet.Network, device);
        NetworkStack.Update();
    }

    /// <summary>
    /// Applies the acknowledged configuration to the device.
    /// </summary>
    /// <param name="device">The device the lease is for.</param>
    /// <param name="packet">The DHCP ACK packet.</param>
    private static void Apply(INetworkDevice device, DhcpPacket packet)
    {
        if (packet.Client is null)
        {
            throw new InvalidOperationException("Parsing DHCP ACK Packet failed, can't apply network configuration.");
        }

        Serial.WriteString("[DHCP ACK] Packet received, applying IP configuration...\n");
        Serial.WriteString($"   Device      : {device.Name}\n");
        Serial.WriteString($"   IP Address  : {packet.Client}\n");
        Serial.WriteString($"   Subnet mask : {packet.Subnet?.ToString() ?? "null"}\n");
        Serial.WriteString($"   Gateway     : {packet.Gateway?.ToString() ?? "null"}\n");
        Serial.WriteString($"   DNS server  : {packet.DNS?.ToString() ?? "null"}\n");

        IPConfig.Enable(device, packet.Client, packet.Subnet ?? new Address4(255, 255, 255, 0), packet.Gateway ?? Address4.Zero);
        if (packet.DNS is not null)
        {
            DnsConfig.Add(packet.DNS);
        }

        Serial.WriteString("[DHCP CONFIG] IP configuration applied.\n");
    }
}
