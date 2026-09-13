using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.ARP;
using Cosmos.Kernel.System.Network.IPv4.TCP;
using Cosmos.Kernel.System.Network.IPv4.UDP;

namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// An IPv4 packet over Ethernet. The build constructors write the complete
/// IPv4 header, including the header checksum, at construction time; the
/// checksum is never recomputed, so the header bytes must not be modified
/// afterwards. Derive from this class to implement a custom IP protocol:
/// pass the protocol number and payload length to a build constructor and
/// write the payload into <see cref="EthernetPacket.RawData"/> from
/// <see cref="DataOffset"/> onward.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class IPPacket : EthernetPacket
{
    /// <summary>Header length in 32-bit words, as parsed from the IHL field.</summary>
    private protected byte _ipHeaderLength;
    private static ushort s_nextFragmentID;

    /// <summary>
    /// Handles a single IPv4 packet.
    /// </summary>
    /// <param name="packetData">The raw data of the packet.</param>
    internal static void IPv4Handler(byte[] packetData)
    {
        Serial.WriteString("[IP] IPv4Handler called\n");

        var ipPacket = new IPPacket(packetData);

        Serial.WriteString("[IP] From ");
        Serial.WriteString(ipPacket.SourceIP.ToString());
        Serial.WriteString(" to ");
        Serial.WriteString(ipPacket.DestinationIP.ToString());
        Serial.WriteString(" proto=");
        Serial.WriteNumber((ulong)ipPacket.Protocol);
        Serial.WriteString("\n");

        ArpCache.Update(ipPacket.SourceIP, ipPacket.SourceMac);

        // Check if packet is for us
        bool isForUs = NetworkStack.AddressMap.ContainsKey(ipPacket.DestinationIP.Id);
        bool isBroadcast = ipPacket.DestinationIP.Parts[3] == 255;

        if (isForUs || isBroadcast)
        {
            switch (ipPacket.Protocol)
            {
                case 1: // ICMP
                    IcmpPacket.ICMPHandler(packetData);
                    break;
                case 6: // TCP
                    TcpPacket.TCPHandler(packetData);
                    break;
                case 17: // UDP
                    UdpPacket.UDPHandler(packetData);
                    break;
            }
        }
        else
        {
            Serial.WriteString("[IP] Packet not for us, dropping\n");
        }
    }

    /// <summary>
    /// Gets the next IP fragment ID. Reading advances the counter.
    /// </summary>
    internal static ushort NextIPFragmentID => s_nextFragmentID++;

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPacket"/> class over
    /// existing frame bytes. The array is aliased, not copied, and neither
    /// the length fields nor the header checksum are validated.
    /// </summary>
    /// <param name="rawData">Raw data.</param>
    public IPPacket(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes all internal fields.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        IPVersion = (byte)((RawData[14] & 0xF0) >> 4);
        _ipHeaderLength = (byte)(RawData[14] & 0x0F);
        TypeOfService = RawData[15];
        IPLength = (ushort)((RawData[16] << 8) | RawData[17]);
        FragmentID = (ushort)((RawData[18] << 8) | RawData[19]);
        IPFlags = (byte)((RawData[20] & 0xE0) >> 5);
        FragmentOffset = (ushort)(((RawData[20] & 0x1F) << 8) | RawData[21]);
        TTL = RawData[22];
        Protocol = RawData[23];
        IPCRC = (ushort)((RawData[24] << 8) | RawData[25]);
        SourceIP = new Address(RawData, 26);
        DestinationIP = new Address(RawData, 30);
        DataOffset = (ushort)(14 + HeaderLength);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPacket"/> class. The
    /// source MAC is looked up from the configured devices by the source IP
    /// (<see cref="MACAddress.None"/> when the address is not configured)
    /// and the destination MAC is left unset for ARP resolution at send
    /// time.
    /// </summary>
    /// <param name="dataLength">Length of the IP payload, in bytes.</param>
    /// <param name="protocol">IP protocol number of the payload.</param>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="flags">Raw value of header byte 20: the 3 flag bits followed by the upper 5 bits of the fragment offset.</param>
    private protected IPPacket(ushort dataLength, byte protocol, Address source, Address dest, byte flags)
        : this(GetSourceMAC(source), MACAddress.None, dataLength, protocol, source, dest, flags)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPacket"/> class with
    /// a known destination MAC (used for broadcast destinations, which skip
    /// ARP resolution).
    /// </summary>
    /// <param name="dataLength">Length of the IP payload, in bytes.</param>
    /// <param name="protocol">IP protocol number of the payload.</param>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="flags">Raw value of header byte 20: the 3 flag bits followed by the upper 5 bits of the fragment offset.</param>
    /// <param name="destMAC">Destination MAC address.</param>
    private protected IPPacket(ushort dataLength, byte protocol, Address source, Address dest, byte flags, MACAddress destMAC)
        : this(GetSourceMAC(source), destMAC, dataLength, protocol, source, dest, flags)
    { }

    /// <summary>
    /// Gets the source MAC address from the NetworkStack based on source IP.
    /// </summary>
    private static MACAddress GetSourceMAC(Address sourceIP)
    {
        if (NetworkStack.AddressMap.TryGetValue(sourceIP.Id, out INetworkDevice? device))
        {
            return device.MacAddress;
        }

        return MACAddress.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IPPacket"/> class,
    /// writing the complete IPv4 header: TTL 0x80, a fragment ID drawn from
    /// a global counter, and the header checksum computed here.
    /// </summary>
    /// <param name="srcMAC">Source MAC address.</param>
    /// <param name="destMAC">Destination MAC address.</param>
    /// <param name="dataLength">Length of the IP payload, in bytes.</param>
    /// <param name="protocol">IP protocol number of the payload.</param>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="flags">Raw value of header byte 20: the 3 flag bits followed by the upper 5 bits of the fragment offset.</param>
    public IPPacket(MACAddress srcMAC, MACAddress destMAC, ushort dataLength, byte protocol,
        Address source, Address dest, byte flags)
        : base(destMAC, srcMAC, 0x0800, dataLength + 14 + 20)
    {
        RawData[14] = 0x45;
        RawData[15] = 0;
        IPLength = (ushort)(dataLength + 20);
        _ipHeaderLength = 5;

        RawData[16] = (byte)((IPLength >> 8) & 0xFF);
        RawData[17] = (byte)((IPLength >> 0) & 0xFF);
        FragmentID = NextIPFragmentID;
        RawData[18] = (byte)((FragmentID >> 8) & 0xFF);
        RawData[19] = (byte)((FragmentID >> 0) & 0xFF);
        RawData[20] = flags;
        RawData[21] = 0x00;
        RawData[22] = 0x80;
        RawData[23] = protocol;
        RawData[24] = 0x00;
        RawData[25] = 0x00;
        for (int b = 0; b < 4; b++)
        {
            RawData[26 + b] = source.Parts[b];
            RawData[30 + b] = dest.Parts[b];
        }
        IPCRC = CalcIPCrc(20);
        RawData[24] = (byte)((IPCRC >> 8) & 0xFF);
        RawData[25] = (byte)((IPCRC >> 0) & 0xFF);

        InitializeFields();
    }

    /// <summary>
    /// Computes the Internet ones'-complement checksum over a range of
    /// <see cref="EthernetPacket.RawData"/>. Available to derived packet
    /// types for their own header checksums.
    /// </summary>
    /// <param name="offset">The offset, in bytes.</param>
    /// <param name="length">The length, in bytes.</param>
    private protected ushort CalcOcCrc(ushort offset, ushort length) => CalcOcCrc(RawData, offset, length);

    /// <summary>
    /// Computes the Internet ones'-complement checksum over a range of the
    /// given buffer.
    /// </summary>
    /// <param name="buffer">The buffer to use.</param>
    /// <param name="offset">The offset, in bytes.</param>
    /// <param name="length">The length, in bytes.</param>
    private protected static ushort CalcOcCrc(byte[] buffer, ushort offset, int length)
    {
        return (ushort)~SumShortValues(buffer, offset, length);
    }

    /// <summary>
    /// Sums a range of the buffer as big-endian 16-bit words with
    /// end-around carry, the accumulation step of the Internet checksum.
    /// </summary>
    /// <param name="buffer">The buffer to use.</param>
    /// <param name="offset">The offset, in bytes.</param>
    /// <param name="length">The length, in bytes.</param>
    private protected static ushort SumShortValues(byte[] buffer, int offset, int length)
    {
        uint chksum = 0;
        int end = offset + (length & ~1);
        int i = offset;

        while (i != end)
        {
            chksum += (uint)(((ushort)buffer[i++] << 8) + (ushort)buffer[i++]);
        }
        if (i != offset + length)
        {
            chksum += (uint)((ushort)buffer[i] << 8);
        }
        chksum = (chksum & 0xFFFF) + (chksum >> 16);
        chksum = (chksum & 0xFFFF) + (chksum >> 16);
        return (ushort)chksum;
    }

    /// <summary>
    /// Computes the IPv4 header checksum over the first
    /// <paramref name="headerLength"/> bytes of the IP header.
    /// </summary>
    /// <param name="headerLength">The length of the header, in bytes.</param>
    private protected ushort CalcIPCrc(ushort headerLength)
    {
        return CalcOcCrc(14, headerLength);
    }

    /// <summary>
    /// Gets the IP version of the packet.
    /// </summary>
    public byte IPVersion { get; private set; }

    /// <summary>
    /// Gets the length of the IP header, in bytes.
    /// </summary>
    public ushort HeaderLength => (ushort)(_ipHeaderLength * 4);

    /// <summary>
    /// Gets the type of service.
    /// </summary>
    public byte TypeOfService { get; private set; }

    /// <summary>
    /// Gets the total length of the IP packet (header plus payload), in bytes.
    /// </summary>
    public ushort IPLength { get; private set; }

    /// <summary>
    /// Gets the fragment ID.
    /// </summary>
    public ushort FragmentID { get; private set; }

    /// <summary>
    /// Gets the 3 flag bits of the packet.
    /// </summary>
    public byte IPFlags { get; private set; }

    /// <summary>
    /// Gets the fragment offset.
    /// </summary>
    public ushort FragmentOffset { get; private set; }

    /// <summary>
    /// Gets the TTL (Time-To-Live) of the packet.
    /// </summary>
    public byte TTL { get; private set; }

    /// <summary>
    /// Gets the IP protocol number of the payload (1 ICMP, 6 TCP, 17 UDP).
    /// </summary>
    public byte Protocol { get; private set; }

    /// <summary>
    /// Gets the IP header checksum as parsed from the header. On locally
    /// built packets it holds the value computed at construction.
    /// </summary>
    public ushort IPCRC { get; private set; }

    /// <summary>
    /// Gets the source IP address.
    /// </summary>
    public Address SourceIP { get; private set; } = null!;

    /// <summary>
    /// Gets the destination IP address.
    /// </summary>
    public Address DestinationIP { get; private set; } = null!;

    /// <summary>
    /// Gets the offset of the IP payload from the start of the frame
    /// (Ethernet header plus IP header), in bytes.
    /// </summary>
    public ushort DataOffset { get; private set; }

    /// <summary>
    /// Gets the length of the IP payload, in bytes.
    /// </summary>
    public ushort DataLength => (ushort)(IPLength - HeaderLength);

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"IP Packet Src={SourceIP}, Dest={DestinationIP}, Protocol={Protocol}, TTL={TTL}, DataLen={DataLength}";
    }
}
