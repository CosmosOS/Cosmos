/*
* PROJECT:          Cosmos OS Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System.Network.IPv4.TCP;

/// <summary>
/// TCP header control flags, as defined by RFC 793.
/// </summary>
[Flags]
[Experimental(Experimentals.PacketSeamDiagId)]
public enum TcpFlags : byte
{
    /// <summary>
    /// FIN: no more data from sender.
    /// </summary>
    FIN = 1 << 0,

    /// <summary>
    /// SYN: synchronize sequence numbers.
    /// </summary>
    SYN = 1 << 1,

    /// <summary>
    /// RST: reset the connection.
    /// </summary>
    RST = 1 << 2,

    /// <summary>
    /// PSH: push function, deliver buffered data to the application.
    /// </summary>
    PSH = 1 << 3,

    /// <summary>
    /// ACK: the acknowledgment number field is significant.
    /// </summary>
    ACK = 1 << 4,

    /// <summary>
    /// URG: the urgent pointer field is significant.
    /// </summary>
    URG = 1 << 5
}

/// <summary>
/// A TCP header option parsed from a received segment.
/// Options are parse products only: the stack reads them from incoming segments but never serializes them into packets it builds.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class TcpOption
{
    /// <summary>
    /// Gets the option kind byte (for example 2 for Maximum Segment Size).
    /// </summary>
    public byte Kind { get; internal set; }

    /// <summary>
    /// Gets the total option length in bytes, including the kind and length bytes. Reads zero for options that carry no length byte.
    /// </summary>
    public byte Length { get; internal set; }

    /// <summary>
    /// Gets the option payload (the bytes after the kind and length bytes), or null when the option has no payload.
    /// </summary>
    public byte[]? Data { get; internal set; }
}

/// <summary>
/// Represents a TCP segment carried in an <see cref="IPPacket"/>.
/// Header properties are snapshots parsed from <see cref="EthernetPacket.RawData"/> at construction and are never recomputed; the checksum
/// of a locally built segment is likewise computed once, in the constructor, so mutating the raw bytes afterwards leaves it stale.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class TcpPacket : IPPacket
{
    /// <summary>
    /// TCP handler.
    /// </summary>
    /// <param name="packetData">Packet data.</param>
    internal static void TCPHandler(byte[] packetData)
    {
        TcpPacket packet = new(packetData);

        if (packet.CheckCRC())
        {
            Tcp? connection = Tcp.GetConnection(packet.DestinationPort, packet.SourcePort, packet.DestinationIP, packet.SourceIP);

            connection?.ReceiveData(packet);
        }
        else
        {
            Serial.WriteString("[TCP] Checksum incorrect, segment dropped.\n");
        }
    }

    /// <summary>
    /// Creates a TCP packet from a received frame.
    /// The array is aliased, not copied: the instance keeps a reference to <paramref name="rawData"/> and parses every header property from it once, during construction.
    /// </summary>
    /// <param name="rawData">Raw frame bytes containing the Ethernet, IPv4 and TCP headers followed by the payload.</param>
    public TcpPacket(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Builds a TCP segment carrying <paramref name="data"/> as payload.
    /// The header bytes and the TCP checksum are written into <see cref="EthernetPacket.RawData"/> here, in the constructor, and never recomputed.
    /// </summary>
    /// <param name="source">Source IPv4 address.</param>
    /// <param name="dest">Destination IPv4 address.</param>
    /// <param name="srcPort">Source TCP port.</param>
    /// <param name="destPort">Destination TCP port.</param>
    /// <param name="sequenceNumber">Sequence number of the first payload byte.</param>
    /// <param name="ackNumber">Acknowledgment number, the next sequence number expected from the peer.</param>
    /// <param name="headerLength">TCP header length in bytes; the stack always passes 20, a header without options.</param>
    /// <param name="flags">Flag bits for the header, a byte cast of combined <see cref="TcpFlags"/> values.</param>
    /// <param name="windowSize">Receive window size to advertise.</param>
    /// <param name="urgentPointer">Urgent pointer field value.</param>
    /// <param name="data">Payload bytes, copied into the segment after the 20 byte header.</param>
    public TcpPacket(Address source, Address dest, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer, byte[] data)
        : base((ushort)(20 + data.Length), 6, source, dest, 0x40)
    {
        AddRawData(data);
        MakePacket(source, dest, srcPort, destPort, sequenceNumber,
        ackNumber, headerLength, flags, windowSize, urgentPointer);
    }

    /// <summary>
    /// Builds an empty TCP segment (header only, no payload), used for control packets such as SYN, ACK and FIN.
    /// The header bytes and the TCP checksum are written into <see cref="EthernetPacket.RawData"/> here, in the constructor, and never recomputed.
    /// </summary>
    /// <param name="source">Source IPv4 address.</param>
    /// <param name="dest">Destination IPv4 address.</param>
    /// <param name="srcPort">Source TCP port.</param>
    /// <param name="destPort">Destination TCP port.</param>
    /// <param name="sequenceNumber">Sequence number for the segment.</param>
    /// <param name="ackNumber">Acknowledgment number, the next sequence number expected from the peer.</param>
    /// <param name="headerLength">TCP header length in bytes; the stack always passes 20, a header without options.</param>
    /// <param name="flags">Flag bits for the header, a byte cast of combined <see cref="TcpFlags"/> values.</param>
    /// <param name="windowSize">Receive window size to advertise.</param>
    /// <param name="urgentPointer">Urgent pointer field value.</param>
    public TcpPacket(Address source, Address dest, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer)
        : base(20, 6, source, dest, 0x40)
    {
        MakePacket(source, dest, srcPort, destPort, sequenceNumber,
        ackNumber, headerLength, flags, windowSize, urgentPointer);
    }

    /// <summary>
    /// Make TCP Packet.
    /// </summary>
    private void MakePacket(Address source, Address dest, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer)
    {
        //ports
        RawData[DataOffset + 0] = (byte)((srcPort >> 8) & 0xFF);
        RawData[DataOffset + 1] = (byte)((srcPort >> 0) & 0xFF);

        RawData[DataOffset + 2] = (byte)((destPort >> 8) & 0xFF);
        RawData[DataOffset + 3] = (byte)((destPort >> 0) & 0xFF);

        //sequence number
        RawData[DataOffset + 4] = (byte)((sequenceNumber >> 24) & 0xFF);
        RawData[DataOffset + 5] = (byte)((sequenceNumber >> 16) & 0xFF);
        RawData[DataOffset + 6] = (byte)((sequenceNumber >> 8) & 0xFF);
        RawData[DataOffset + 7] = (byte)((sequenceNumber >> 0) & 0xFF);

        //Acknowledgment number
        RawData[DataOffset + 8] = (byte)((ackNumber >> 24) & 0xFF);
        RawData[DataOffset + 9] = (byte)((ackNumber >> 16) & 0xFF);
        RawData[DataOffset + 10] = (byte)((ackNumber >> 8) & 0xFF);
        RawData[DataOffset + 11] = (byte)((ackNumber >> 0) & 0xFF);

        //Header length
        RawData[DataOffset + 12] = (byte)(((headerLength >> 0) & 0xFF) * 4);

        //Flags
        RawData[DataOffset + 13] = (byte)((flags >> 0) & 0xFF);

        //Window size value
        RawData[DataOffset + 14] = (byte)((windowSize >> 8) & 0xFF);
        RawData[DataOffset + 15] = (byte)((windowSize >> 0) & 0xFF);

        //Checksum
        RawData[DataOffset + 16] = 0;
        RawData[DataOffset + 17] = 0;

        //Urgent Pointer
        RawData[DataOffset + 18] = (byte)((urgentPointer >> 8) & 0xFF);
        RawData[DataOffset + 19] = (byte)((urgentPointer >> 0) & 0xFF);

        InitializeFields();

        //Checksum computation
        byte[] header = MakeHeader();
        ushort calculatedcrc = CalcOcCrc(header, 0, header.Length);

        //Checksum
        RawData[DataOffset + 16] = (byte)((calculatedcrc >> 8) & 0xFF);
        RawData[DataOffset + 17] = (byte)((calculatedcrc >> 0) & 0xFF);
    }

    /// <summary>
    /// Parses the TCP header fields from <see cref="EthernetPacket.RawData"/> into the header properties, and the header options into <see cref="Options"/> when the header is longer than 20 bytes.
    /// Runs during construction; the properties are snapshots and do not track later changes to the raw bytes.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        SourcePort = (ushort)((RawData[DataOffset] << 8) | RawData[DataOffset + 1]);
        DestinationPort = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
        SequenceNumber = (uint)((RawData[DataOffset + 4] << 24) | (RawData[DataOffset + 5] << 16) | (RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
        AckNumber = (uint)((RawData[DataOffset + 8] << 24) | (RawData[DataOffset + 9] << 16) | (RawData[DataOffset + 10] << 8) | RawData[DataOffset + 11]);
        TcpHeaderLength = (byte)((RawData[DataOffset + 12] >> 4) * 4);
        FlagBits = RawData[DataOffset + 13];
        WindowSize = (ushort)((RawData[DataOffset + 14] << 8) | RawData[DataOffset + 15]);
        Checksum = (ushort)((RawData[DataOffset + 16] << 8) | RawData[DataOffset + 17]);
        UrgentPointer = (ushort)((RawData[DataOffset + 18] << 8) | RawData[DataOffset + 19]);

        _syn = (RawData[47] & (byte)TcpFlags.SYN) != 0;
        _ack = (RawData[47] & (byte)TcpFlags.ACK) != 0;
        _fin = (RawData[47] & (byte)TcpFlags.FIN) != 0;
        _psh = (RawData[47] & (byte)TcpFlags.PSH) != 0;
        _rst = (RawData[47] & (byte)TcpFlags.RST) != 0;
        _urg = (RawData[47] & (byte)TcpFlags.URG) != 0;

        if (TcpHeaderLength > 20) //options
        {
            Options = new List<TcpOption>();

            for (int i = 0; i < TcpDataLength; i++)
            {
                TcpOption option = new();
                option.Kind = RawData[DataOffset + 20 + i];

                if (option.Kind != 1) //NOP
                {
                    option.Length = RawData[DataOffset + 20 + i + 1];

                    if (option.Length != 2)
                    {
                        option.Data = new byte[option.Length - 2];
                        for (int j = 0; j < option.Length - 2; j++)
                        {
                            option.Data[j] = RawData[DataOffset + 20 + i + 2 + j];
                        }
                    }

                    Options.Add(option);

                    i += option.Length - 1;
                }
            }
        }
    }

    /// <summary>
    /// Add raw data to TCP Packet.
    /// </summary>
    internal void AddRawData(byte[] raw)
    {
        for (int i = 0; i < raw.Length; i++)
        {
            RawData[DataOffset + 20 + i] = raw[i];
        }
    }

    /// <summary>
    /// Make TCP Header for CRC Computation
    /// </summary>
    internal byte[] MakeHeader()
    {
        byte[] header = new byte[12 + TcpHeaderLength + TcpDataLength];

        /* Pseudo Header */
        //Addresses
        for (int b = 0; b < 4; b++)
        {
            header[0 + b] = SourceIP.Parts[b];
            header[4 + b] = DestinationIP.Parts[b];
        }
        //Reserved
        header[8] = 0x00;
        //Protocol (TCP)
        header[9] = 0x06;
        ushort tcplen = (ushort)(TcpHeaderLength + TcpDataLength);
        //TCP Length
        header[10] = (byte)((tcplen >> 8) & 0xFF);
        header[11] = (byte)((tcplen >> 0) & 0xFF);

        /* TCP Packet */
        for (int i = 0; i < tcplen; i++)
        {
            header[12 + i] = RawData[DataOffset + i];
        }

        return header;
    }

    /// <summary>
    /// Verifies the checksum the segment arrived with. The ones'-complement
    /// sum over the pseudo-header and the whole segment, the checksum field
    /// included, is all ones when the bytes are intact, so the complement
    /// <c>CalcOcCrc</c> returns is zero.
    /// </summary>
    private bool CheckCRC()
    {
        byte[] header = MakeHeader();
        return CalcOcCrc(header, 0, header.Length) == 0;
    }

    /// <summary>
    /// Gets the TCP options parsed from a received segment's header, or null when the header is 20 bytes and carries none.
    /// Options are parse products only: they are never serialized into packets this stack builds.
    /// </summary>
    public List<TcpOption>? Options { get; internal set; }

    /// <summary>
    /// Is SYN Flag set.
    /// </summary>
    internal bool _syn;
    /// <summary>
    /// Is ACK Flag set.
    /// </summary>
    internal bool _ack;
    /// <summary>
    /// Is FIN Flag set.
    /// </summary>
    internal bool _fin;
    /// <summary>
    /// Is PSH Flag set.
    /// </summary>
    internal bool _psh;
    /// <summary>
    /// Is RST Flag set.
    /// </summary>
    internal bool _rst;
    /// <summary>
    /// Is URG Flag set.
    /// </summary>
    internal bool _urg;

    /// <summary>
    /// Gets the destination port, a snapshot parsed from the header at construction.
    /// </summary>
    public ushort DestinationPort { get; private set; }
    /// <summary>
    /// Gets the source port, a snapshot parsed from the header at construction.
    /// </summary>
    public ushort SourcePort { get; private set; }
    /// <summary>
    /// Gets the acknowledgment number, a snapshot parsed from the header at construction.
    /// </summary>
    public uint AckNumber { get; private set; }
    /// <summary>
    /// Gets the sequence number, a snapshot parsed from the header at construction.
    /// </summary>
    public uint SequenceNumber { get; private set; }
    /// <summary>
    /// Gets the TCP header length in bytes, decoded from the data offset field at construction.
    /// </summary>
    public byte TcpHeaderLength { get; private set; }
    /// <summary>
    /// Gets the raw flag byte from the header, a snapshot taken at construction; see <see cref="TcpFlags"/> for the bit values.
    /// </summary>
    public byte FlagBits { get; private set; }
    /// <summary>
    /// Gets the advertised window size, a snapshot parsed from the header at construction.
    /// </summary>
    public ushort WindowSize { get; private set; }
    /// <summary>
    /// Gets the checksum field as parsed from the header at construction.
    /// On locally built packets this reads zero: the computed checksum bytes are patched into <see cref="EthernetPacket.RawData"/> after the parse pass, so the snapshot never sees them.
    /// </summary>
    public ushort Checksum { get; private set; }
    /// <summary>
    /// Gets the urgent pointer, a snapshot parsed from the header at construction.
    /// </summary>
    public ushort UrgentPointer { get; private set; }

    /// <summary>
    /// Gets the payload length in bytes: the IP total length minus the IP header length minus the TCP header length, computed from the header snapshots.
    /// </summary>
    public ushort TcpDataLength => (ushort)(IPLength - HeaderLength - TcpHeaderLength);

    /// <summary>
    /// Get TCP data.
    /// </summary>
    internal byte[] TcpData
    {
        get
        {
            byte[] data = new byte[TcpDataLength];

            for (int b = 0; b < data.Length; b++)
            {
                data[b] = RawData[DataOffset + TcpHeaderLength + b];
            }

            return data;
        }
    }

    /// <summary>
    /// Returns the names of the flags set on this segment, pipe separated (for example "SYN|ACK"), or an empty string when none are set.
    /// </summary>
    /// <returns>The flag names joined with a pipe character.</returns>
    public string GetFlags()
    {
        string flags = "";

        if (_fin)
        {
            flags += "FIN|";
        }

        if (_syn)
        {
            flags += "SYN|";
        }

        if (_rst)
        {
            flags += "RST|";
        }

        if (_psh)
        {
            flags += "PSH|";
        }

        if (_ack)
        {
            flags += "ACK|";
        }

        if (_urg)
        {
            flags += "URG|";
        }

        if (flags.Length > 0)
        {
            return flags.Substring(0, flags.Length - 1);
        }
        return flags;
    }

    /// <summary>
    /// Returns a string describing the segment: source and destination endpoints, flags, sequence number and acknowledgment number.
    /// </summary>
    /// <returns>A human readable summary of the segment.</returns>
    public override string ToString()
    {
        return "TCP Packet " + SourceIP + ":" + SourcePort +
            " -> " + DestinationIP + ":" + DestinationPort + " (flags=" + GetFlags() +
            ", seq=" + SequenceNumber + ", ack=" + AckNumber + ")";
    }
}
