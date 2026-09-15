// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4.DHCP;

namespace Cosmos.Kernel.System.Network.UDP;

/// <summary>
/// Delegate for UDP data received events.
/// </summary>
internal delegate void UdpDataReceivedHandler(UdpPacket packet);

/// <summary>
/// A UDP datagram, over IPv4 or over IPv6. The wire header is the same on
/// both, so one class serves both: the datagram composes the
/// <see cref="InternetPacket"/> that carries it rather than deriving from a
/// version-specific packet, and reads the frame buffer, the addresses and
/// the payload offset back through <see cref="Network"/>.
/// </summary>
/// <remarks>
/// Header properties are snapshots parsed from <see cref="RawData"/> when the
/// packet is constructed; they are not re-read afterwards. The checksum
/// differs by version, and this is the only place the two differ: a zero
/// checksum means "not computed" over IPv4 and is illegal over IPv6, so the
/// build constructors that receive the payload write a real checksum, and the
/// ones that leave the payload for the caller leave the field zero for
/// <see cref="WriteChecksum"/> to fill in.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class UdpPacket
{
    /// <summary>Length of the UDP header, in bytes.</summary>
    internal const ushort UdpHeaderLength = 8;

    /// <summary>The BOOTP/DHCP client port, which DHCP replies are addressed to.</summary>
    private const ushort DhcpClientPort = 68;

    /// <summary>
    /// Callback for receiving UDP data.
    /// </summary>
    internal static UdpDataReceivedHandler? OnUDPDataReceived { get; set; }

    /// <summary>
    /// Handles a received UDP datagram, of either version: a datagram whose
    /// checksum does not verify is dropped, a datagram addressed to the DHCP
    /// client port goes to the DHCP handler, and everything else goes to the
    /// client bound to its destination port.
    /// </summary>
    /// <param name="network">The parsed internet packet carrying the datagram.</param>
    internal static void UDPHandler(InternetPacket network)
    {
        UdpPacket udpPacket = new(network);

        Serial.WriteString("[UDP] Received from ");
        Serial.WriteString(udpPacket.SourceIP.ToString());
        Serial.WriteString(":");
        Serial.WriteNumber((ulong)udpPacket.SourcePort);
        Serial.WriteString(" -> ");
        Serial.WriteNumber((ulong)udpPacket.DestinationPort);
        Serial.WriteString(" len=");
        Serial.WriteNumber((ulong)udpPacket.UdpDataLength);
        Serial.WriteString("\n");

        if (!udpPacket.VerifyChecksum())
        {
            Serial.WriteString("[UDP] Bad checksum, dropping\n");
            return;
        }

        // Route to specific protocol handlers based on port
        if (udpPacket.DestinationPort == DhcpClientPort)
        {
            DhcpPacket.DHCPHandler(udpPacket.RawData);
        }
        else
        {
            // Route to UdpClient if available. DNS used to be routed here a
            // second time as well, which enqueued every reply twice into the
            // one client bound to port 53: the client dequeued one and the
            // stale copy then satisfied the next query's wait, so every second
            // lookup on a DnsClient failed its own query-name check.
            UdpClient? client = UdpClient.GetClient(udpPacket.DestinationPort);
            client?.ReceiveData(udpPacket);
        }

        // Call the registered callback if any
        OnUDPDataReceived?.Invoke(udpPacket);
    }

    /// <summary>
    /// Parses a UDP datagram from a raw Ethernet frame, picking the internet
    /// version from the EtherType. The instance aliases
    /// <paramref name="rawData"/> without copying, so later changes to the
    /// array are visible through this packet. Header properties are parsed
    /// once during construction.
    /// </summary>
    /// <param name="rawData">The raw frame bytes, starting at the Ethernet header.</param>
    /// <exception cref="ArgumentException">The frame carries neither an IPv4 nor an IPv6 header.</exception>
    public UdpPacket(byte[] rawData)
        : this(InternetPacket.Parse(rawData))
    {
    }

    /// <summary>
    /// Parses a UDP datagram out of an already parsed internet packet, which
    /// is what the receive path does: the network header is parsed once and
    /// the transport reads its own section out of the same frame.
    /// </summary>
    /// <param name="network">The internet packet carrying the datagram.</param>
    internal UdpPacket(InternetPacket network)
    {
        Network = network;
        InitializeFields();
    }

    /// <summary>
    /// Creates a UDP datagram with an uninitialized payload area of
    /// <paramref name="dataLength"/> bytes, of the version both addresses
    /// belong to. Every header and length field is written during
    /// construction and never recomputed, so the payload must be filled in
    /// before the packet is sent. The checksum field is left zero: call
    /// <see cref="WriteChecksum"/> once the payload is in place, which is
    /// mandatory over IPv6 and optional over IPv4.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address.</param>
    /// <param name="srcPort">The source port.</param>
    /// <param name="destPort">The destination port.</param>
    /// <param name="dataLength">The payload length in bytes.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public UdpPacket(Address source, Address dest, ushort srcPort, ushort destPort, ushort dataLength)
        : this(Build(source, dest, dataLength), srcPort, destPort, dataLength)
    {
    }

    /// <summary>
    /// Creates a UDP datagram with an uninitialized payload area of
    /// <paramref name="dataLength"/> bytes and a preset destination MAC
    /// address, which skips neighbor resolution. Every header and length field
    /// is written during construction and never recomputed, so the payload
    /// must be filled in before the packet is sent. The checksum field is left
    /// zero: call <see cref="WriteChecksum"/> once the payload is in place,
    /// which is mandatory over IPv6 and optional over IPv4.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address.</param>
    /// <param name="srcPort">The source port.</param>
    /// <param name="destPort">The destination port.</param>
    /// <param name="dataLength">The payload length in bytes.</param>
    /// <param name="destMac">The destination MAC address to write into the Ethernet header.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public UdpPacket(Address source, Address dest, ushort srcPort, ushort destPort, ushort dataLength, MACAddress destMac)
        : this(Build(source, dest, dataLength, destMac), srcPort, destPort, dataLength)
    {
    }

    /// <summary>
    /// Creates a UDP datagram and copies <paramref name="data"/> into its
    /// payload area, of the version both addresses belong to. Every header and
    /// length field is written during construction, the checksum over the
    /// complete datagram included, and none is recomputed: writes to the
    /// buffer afterwards leave the checksum stale.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address.</param>
    /// <param name="srcPort">The source port.</param>
    /// <param name="destPort">The destination port.</param>
    /// <param name="data">The payload bytes to copy into the packet.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public UdpPacket(Address source, Address dest, ushort srcPort, ushort destPort, byte[] data)
        : this(source, dest, srcPort, destPort, (ushort)data.Length)
    {
        WritePayload(data);
    }

    /// <summary>
    /// Creates a UDP datagram with a copied payload and a preset destination
    /// MAC address, which skips neighbor resolution. Every header and length
    /// field is written during construction, the checksum over the complete
    /// datagram included, and none is recomputed: writes to the buffer
    /// afterwards leave the checksum stale.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address.</param>
    /// <param name="srcPort">The source port.</param>
    /// <param name="destPort">The destination port.</param>
    /// <param name="data">The payload bytes to copy into the packet.</param>
    /// <param name="destMac">The destination MAC address to write into the Ethernet header.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public UdpPacket(Address source, Address dest, ushort srcPort, ushort destPort, byte[] data, MACAddress destMac)
        : this(source, dest, srcPort, destPort, (ushort)data.Length, destMac)
    {
        WritePayload(data);
    }

    /// <summary>
    /// Writes the UDP header of a datagram being built and takes the first
    /// header snapshot.
    /// </summary>
    private UdpPacket(InternetPacket network, ushort srcPort, ushort destPort, ushort dataLength)
    {
        Network = network;
        WriteHeader(srcPort, destPort, dataLength);
        InitializeFields();
    }

    /// <summary>
    /// Builds the internet packet for a datagram of <paramref name="dataLength"/> payload bytes.
    /// </summary>
    private static InternetPacket Build(Address source, Address dest, ushort dataLength)
    {
        return InternetPacket.CreateForTransport(source, dest, InternetPacket.ProtocolUdp,
            (ushort)(dataLength + UdpHeaderLength), false);
    }

    /// <summary>
    /// Builds the internet packet for a datagram of <paramref name="dataLength"/> payload bytes,
    /// with the destination MAC address already known.
    /// </summary>
    private static InternetPacket Build(Address source, Address dest, ushort dataLength, MACAddress destMac)
    {
        return InternetPacket.CreateForTransport(source, dest, InternetPacket.ProtocolUdp,
            (ushort)(dataLength + UdpHeaderLength), false, destMac);
    }

    /// <summary>
    /// The internet packet carrying this datagram: the frame buffer, the
    /// addresses, and the version-specific half of the checksum. Pass it to
    /// <see cref="NetworkStack.Send"/> to transmit the datagram.
    /// </summary>
    public InternetPacket Network { get; }

    /// <summary>
    /// The complete wire image of the frame, Ethernet header included.
    /// </summary>
    public byte[] RawData => Network.RawData;

    /// <summary>
    /// The source address of the carrying internet packet.
    /// </summary>
    public Address SourceIP => Network.SourceIP;

    /// <summary>
    /// The destination address of the carrying internet packet.
    /// </summary>
    public Address DestinationIP => Network.DestinationIP;

    /// <summary>
    /// The offset of the UDP header from the start of the frame.
    /// </summary>
    private protected ushort DataOffset => Network.DataOffset;

    /// <summary>
    /// Gets the destination port, a snapshot parsed from the UDP header at construction.
    /// </summary>
    public ushort DestinationPort { get; private set; }

    /// <summary>
    /// Gets the source port, a snapshot parsed from the UDP header at construction.
    /// </summary>
    public ushort SourcePort { get; private set; }

    /// <summary>
    /// Gets the value of the UDP length field: the 8-byte UDP header plus the payload. It is
    /// a snapshot parsed from the header at construction and is never recomputed.
    /// </summary>
    public ushort UdpLength { get; private set; }

    /// <summary>
    /// Gets the checksum field as parsed from the header, a snapshot taken at
    /// construction. Reads zero on a datagram whose payload the caller still
    /// has to fill in and checksum.
    /// </summary>
    public ushort Checksum { get; private set; }

    /// <summary>
    /// Gets the payload length in bytes: <see cref="UdpLength"/> minus the 8-byte UDP header.
    /// </summary>
    public ushort UdpDataLength => (ushort)(UdpLength - UdpHeaderLength);

    /// <summary>
    /// Writes the UDP header. The length field counts the header, and the
    /// checksum field is left zero for <see cref="WriteChecksum"/>.
    /// </summary>
    private void WriteHeader(ushort srcPort, ushort destPort, ushort dataLength)
    {
        ushort offset = DataOffset;
        ushort length = (ushort)(dataLength + UdpHeaderLength);

        RawData[offset + 0] = (byte)(srcPort >> 8);
        RawData[offset + 1] = (byte)srcPort;
        RawData[offset + 2] = (byte)(destPort >> 8);
        RawData[offset + 3] = (byte)destPort;
        RawData[offset + 4] = (byte)(length >> 8);
        RawData[offset + 5] = (byte)length;
        RawData[offset + 6] = 0;
        RawData[offset + 7] = 0;
    }

    /// <summary>
    /// Copies the payload in after the header and settles the checksum over
    /// the finished datagram.
    /// </summary>
    private void WritePayload(byte[] data)
    {
        data.CopyTo(RawData.AsSpan(DataOffset + UdpHeaderLength, data.Length));
        WriteChecksum();
    }

    /// <summary>
    /// Parses the UDP header fields (source port, destination port, length and
    /// checksum) from <see cref="RawData"/> into the header properties. Runs
    /// during construction; the properties are snapshots and are not refreshed
    /// afterwards.
    /// </summary>
    private protected virtual void InitializeFields()
    {
        ushort offset = DataOffset;
        SourcePort = (ushort)((RawData[offset] << 8) | RawData[offset + 1]);
        DestinationPort = (ushort)((RawData[offset + 2] << 8) | RawData[offset + 3]);
        UdpLength = (ushort)((RawData[offset + 4] << 8) | RawData[offset + 5]);
        Checksum = (ushort)((RawData[offset + 6] << 8) | RawData[offset + 7]);
    }

    /// <summary>
    /// Computes the checksum over the datagram as it stands and stores it,
    /// then re-parses the header snapshots. Call this once the payload is
    /// complete, which is mandatory over IPv6, where a zero checksum is
    /// illegal, and optional over IPv4, where it means "not computed". A
    /// computed value of zero is stored as all ones, as RFC 768 requires, so
    /// that it is never mistaken for an absent checksum.
    /// </summary>
    public void WriteChecksum()
    {
        ushort offset = DataOffset;
        RawData[offset + 6] = 0;
        RawData[offset + 7] = 0;

        ushort checksum = Network.ComputeTransportChecksum(InternetPacket.ProtocolUdp, UdpLength);
        if (checksum == 0)
        {
            checksum = 0xFFFF;
        }

        RawData[offset + 6] = (byte)(checksum >> 8);
        RawData[offset + 7] = (byte)checksum;
        InitializeFields();
    }

    /// <summary>
    /// Checks the stored checksum against the datagram and the pseudo-header,
    /// as a receiver does. Sums the whole datagram on every call.
    /// </summary>
    /// <returns>True when the datagram is intact. A stored zero counts as
    /// intact over IPv4, where it means the sender computed no checksum, and
    /// as corrupt over IPv6, where every datagram must carry one.</returns>
    public bool VerifyChecksum()
    {
        if (Checksum == 0)
        {
            return !Network.TransportChecksumRequired;
        }

        if (UdpLength < UdpHeaderLength || DataOffset + UdpLength > RawData.Length)
        {
            return false;
        }

        return Network.ComputeTransportChecksum(InternetPacket.ProtocolUdp, UdpLength) == 0;
    }

    /// <summary>
    /// Returns a fresh copy of the UDP payload (the bytes after the 8-byte UDP
    /// header). Each call allocates; the packet's underlying buffer is not
    /// exposed, so mutating the returned array does not affect the packet.
    /// </summary>
    /// <returns>A new array holding the payload bytes.</returns>
    public byte[] GetUdpData()
    {
        byte[] data = new byte[UdpDataLength];
        RawData.AsSpan(DataOffset + UdpHeaderLength, data.Length).CopyTo(data);
        return data;
    }

    /// <summary>
    /// Returns a string with the source and destination endpoints and the payload length.
    /// </summary>
    /// <returns>A human-readable summary of the packet.</returns>
    public override string ToString()
    {
        return $"UDP Packet Src={SourceIP}:{SourcePort},Dest={DestinationIP}:{DestinationPort}, DataLen={UdpDataLength}";
    }
}
