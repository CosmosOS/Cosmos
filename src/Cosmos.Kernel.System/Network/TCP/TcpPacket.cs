/*
* PROJECT:          Cosmos OS Development
* CONTENT:          TCP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System.Network.TCP;

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
/// A TCP segment, over IPv4 or over IPv6. The wire header is the same on
/// both, so one class serves both: the segment composes the
/// <see cref="InternetPacket"/> that carries it rather than deriving from a
/// version-specific packet, and reads the frame buffer, the addresses and
/// the segment offset back through <see cref="Network"/>. The one part that
/// differs by version is the checksum pseudo-header, which the internet
/// packet supplies.
/// </summary>
/// <remarks>
/// Header properties are snapshots parsed from <see cref="RawData"/> at
/// construction and are never recomputed; the checksum of a locally built
/// segment is likewise computed once, in the constructor, so mutating the raw
/// bytes afterwards leaves it stale.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class TcpPacket
{
    /// <summary>Length of a TCP header without options, in bytes.</summary>
    internal const ushort TcpHeaderMinimumLength = 20;

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
    /// Handles a received TCP segment, of either version. A segment whose
    /// checksum does not verify is dropped; the rest go to the connection
    /// their four-tuple names.
    /// </summary>
    /// <param name="network">The parsed internet packet carrying the segment.</param>
    internal static void TCPHandler(InternetPacket network)
    {
        TcpPacket packet = new(network);

        if (packet.VerifyChecksum())
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
    /// Creates a TCP segment from a received frame, picking the internet
    /// version from the EtherType. The array is aliased, not copied: the
    /// instance keeps a reference to <paramref name="rawData"/> and parses
    /// every header property from it once, during construction.
    /// </summary>
    /// <param name="rawData">Raw frame bytes containing the Ethernet, internet and TCP headers followed by the payload.</param>
    /// <exception cref="ArgumentException">The frame carries neither an IPv4 nor an IPv6 header.</exception>
    public TcpPacket(byte[] rawData)
        : this(InternetPacket.Parse(rawData))
    { }

    /// <summary>
    /// Parses a TCP segment out of an already parsed internet packet, which is
    /// what the receive path does: the network header is parsed once and the
    /// transport reads its own section out of the same frame.
    /// </summary>
    /// <param name="network">The internet packet carrying the segment.</param>
    internal TcpPacket(InternetPacket network)
    {
        Network = network;
        InitializeFields();
    }

    /// <summary>
    /// Builds a TCP segment carrying <paramref name="data"/> as payload, of
    /// the version both addresses belong to.
    /// The header bytes and the TCP checksum are written into <see cref="RawData"/> here, in the constructor, and never recomputed.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="srcPort">Source TCP port.</param>
    /// <param name="destPort">Destination TCP port.</param>
    /// <param name="sequenceNumber">Sequence number of the first payload byte.</param>
    /// <param name="ackNumber">Acknowledgment number, the next sequence number expected from the peer.</param>
    /// <param name="headerLength">TCP header length in bytes; the stack always passes 20, a header without options.</param>
    /// <param name="flags">Flag bits for the header, a byte cast of combined <see cref="TcpFlags"/> values.</param>
    /// <param name="windowSize">Receive window size to advertise.</param>
    /// <param name="urgentPointer">Urgent pointer field value.</param>
    /// <param name="data">Payload bytes, copied into the segment after the 20 byte header.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public TcpPacket(Address source, Address dest, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer, byte[] data)
        : this(Build(source, dest, (ushort)(TcpHeaderMinimumLength + data.Length)), srcPort, destPort,
            sequenceNumber, ackNumber, headerLength, flags, windowSize, urgentPointer, data)
    { }

    /// <summary>
    /// Builds an empty TCP segment (header only, no payload), used for control packets such as SYN, ACK and FIN,
    /// of the version both addresses belong to.
    /// The header bytes and the TCP checksum are written into <see cref="RawData"/> here, in the constructor, and never recomputed.
    /// </summary>
    /// <param name="source">Source address.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="srcPort">Source TCP port.</param>
    /// <param name="destPort">Destination TCP port.</param>
    /// <param name="sequenceNumber">Sequence number for the segment.</param>
    /// <param name="ackNumber">Acknowledgment number, the next sequence number expected from the peer.</param>
    /// <param name="headerLength">TCP header length in bytes; the stack always passes 20, a header without options.</param>
    /// <param name="flags">Flag bits for the header, a byte cast of combined <see cref="TcpFlags"/> values.</param>
    /// <param name="windowSize">Receive window size to advertise.</param>
    /// <param name="urgentPointer">Urgent pointer field value.</param>
    /// <exception cref="ArgumentException">The two addresses belong to different IP versions.</exception>
    public TcpPacket(Address source, Address dest, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer)
        : this(Build(source, dest, TcpHeaderMinimumLength), srcPort, destPort,
            sequenceNumber, ackNumber, headerLength, flags, windowSize, urgentPointer, null)
    { }

    /// <summary>
    /// Writes the payload, then the TCP header and its checksum, of a segment
    /// being built.
    /// </summary>
    private TcpPacket(InternetPacket network, ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer, byte[]? data)
    {
        Network = network;

        if (data is not null)
        {
            AddRawData(data);
        }

        MakePacket(srcPort, destPort, sequenceNumber, ackNumber, headerLength, flags, windowSize, urgentPointer);
    }

    /// <summary>
    /// Builds the internet packet for a segment of <paramref name="segmentLength"/>
    /// bytes. TCP sets Don't Fragment on IPv4, which IPv6 has no equivalent of.
    /// </summary>
    private static InternetPacket Build(Address source, Address dest, ushort segmentLength)
    {
        return InternetPacket.CreateForTransport(source, dest, InternetPacket.ProtocolTcp, segmentLength, true);
    }

    /// <summary>
    /// The internet packet carrying this segment: the frame buffer, the
    /// addresses, and the version-specific half of the checksum. Pass it to
    /// <see cref="NetworkStack.Send"/> to transmit the segment.
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
    /// The offset of the TCP header from the start of the frame.
    /// </summary>
    private protected ushort DataOffset => Network.DataOffset;

    /// <summary>
    /// Make TCP Packet.
    /// </summary>
    private void MakePacket(ushort srcPort, ushort destPort,
        uint sequenceNumber, uint ackNumber, ushort headerLength, byte flags,
        ushort windowSize, ushort urgentPointer)
    {
        ushort offset = DataOffset;

        //ports
        RawData[offset + 0] = (byte)((srcPort >> 8) & 0xFF);
        RawData[offset + 1] = (byte)((srcPort >> 0) & 0xFF);

        RawData[offset + 2] = (byte)((destPort >> 8) & 0xFF);
        RawData[offset + 3] = (byte)((destPort >> 0) & 0xFF);

        //sequence number
        RawData[offset + 4] = (byte)((sequenceNumber >> 24) & 0xFF);
        RawData[offset + 5] = (byte)((sequenceNumber >> 16) & 0xFF);
        RawData[offset + 6] = (byte)((sequenceNumber >> 8) & 0xFF);
        RawData[offset + 7] = (byte)((sequenceNumber >> 0) & 0xFF);

        //Acknowledgment number
        RawData[offset + 8] = (byte)((ackNumber >> 24) & 0xFF);
        RawData[offset + 9] = (byte)((ackNumber >> 16) & 0xFF);
        RawData[offset + 10] = (byte)((ackNumber >> 8) & 0xFF);
        RawData[offset + 11] = (byte)((ackNumber >> 0) & 0xFF);

        //Header length
        RawData[offset + 12] = (byte)(((headerLength >> 0) & 0xFF) * 4);

        //Flags
        RawData[offset + 13] = (byte)((flags >> 0) & 0xFF);

        //Window size value
        RawData[offset + 14] = (byte)((windowSize >> 8) & 0xFF);
        RawData[offset + 15] = (byte)((windowSize >> 0) & 0xFF);

        //Checksum
        RawData[offset + 16] = 0;
        RawData[offset + 17] = 0;

        //Urgent Pointer
        RawData[offset + 18] = (byte)((urgentPointer >> 8) & 0xFF);
        RawData[offset + 19] = (byte)((urgentPointer >> 0) & 0xFF);

        InitializeFields();
        WriteChecksum();
    }

    /// <summary>
    /// Parses the TCP header fields from <see cref="RawData"/> into the header properties, and the header options into <see cref="Options"/> when the header is longer than 20 bytes.
    /// Runs during construction; the properties are snapshots and do not track later changes to the raw bytes.
    /// </summary>
    private void InitializeFields()
    {
        ushort offset = DataOffset;

        SourcePort = (ushort)((RawData[offset] << 8) | RawData[offset + 1]);
        DestinationPort = (ushort)((RawData[offset + 2] << 8) | RawData[offset + 3]);
        SequenceNumber = (uint)((RawData[offset + 4] << 24) | (RawData[offset + 5] << 16) | (RawData[offset + 6] << 8) | RawData[offset + 7]);
        AckNumber = (uint)((RawData[offset + 8] << 24) | (RawData[offset + 9] << 16) | (RawData[offset + 10] << 8) | RawData[offset + 11]);
        TcpHeaderLength = (byte)((RawData[offset + 12] >> 4) * 4);
        FlagBits = RawData[offset + 13];
        WindowSize = (ushort)((RawData[offset + 14] << 8) | RawData[offset + 15]);
        Checksum = (ushort)((RawData[offset + 16] << 8) | RawData[offset + 17]);
        UrgentPointer = (ushort)((RawData[offset + 18] << 8) | RawData[offset + 19]);

        // Read the flag bits back from the field just parsed. They used to be
        // read from a hardcoded frame offset, which is the flags byte only
        // when the IPv4 header carries no options and never for IPv6.
        _syn = (FlagBits & (byte)TcpFlags.SYN) != 0;
        _ack = (FlagBits & (byte)TcpFlags.ACK) != 0;
        _fin = (FlagBits & (byte)TcpFlags.FIN) != 0;
        _psh = (FlagBits & (byte)TcpFlags.PSH) != 0;
        _rst = (FlagBits & (byte)TcpFlags.RST) != 0;
        _urg = (FlagBits & (byte)TcpFlags.URG) != 0;

        if (TcpHeaderLength > TcpHeaderMinimumLength) //options
        {
            Options = [];

            for (int i = 0; i < TcpDataLength; i++)
            {
                TcpOption option = new()
                {
                    Kind = RawData[offset + 20 + i]
                };

                if (option.Kind != 1) //NOP
                {
                    option.Length = RawData[offset + 20 + i + 1];

                    if (option.Length != 2)
                    {
                        option.Data = new byte[option.Length - 2];
                        for (int j = 0; j < option.Length - 2; j++)
                        {
                            option.Data[j] = RawData[offset + 20 + i + 2 + j];
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
        raw.CopyTo(RawData.AsSpan(DataOffset + TcpHeaderMinimumLength, raw.Length));
    }

    /// <summary>
    /// Computes the checksum over the segment as it stands, stores it, and
    /// re-parses the header snapshots. Called once the header and payload are
    /// written; nothing recomputes it afterwards.
    /// </summary>
    private void WriteChecksum()
    {
        ushort offset = DataOffset;
        RawData[offset + 16] = 0;
        RawData[offset + 17] = 0;

        ushort checksum = Network.ComputeTransportChecksum(InternetPacket.ProtocolTcp, Network.DataLength);

        RawData[offset + 16] = (byte)((checksum >> 8) & 0xFF);
        RawData[offset + 17] = (byte)((checksum >> 0) & 0xFF);
        InitializeFields();
    }

    /// <summary>
    /// Verifies the checksum the segment arrived with. The ones' complement
    /// sum over the pseudo-header and the whole segment, the checksum field
    /// included, is all ones when the bytes are intact, so the complement is
    /// zero.
    /// </summary>
    /// <returns>True when the segment is intact.</returns>
    public bool VerifyChecksum()
    {
        if (Network.DataLength < TcpHeaderMinimumLength || DataOffset + Network.DataLength > RawData.Length)
        {
            return false;
        }

        return Network.ComputeTransportChecksum(InternetPacket.ProtocolTcp, Network.DataLength) == 0;
    }

    /// <summary>
    /// Gets the TCP options parsed from a received segment's header, or null when the header is 20 bytes and carries none.
    /// Options are parse products only: they are never serialized into packets this stack builds.
    /// </summary>
    public List<TcpOption>? Options { get; internal set; }

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
    /// Gets the checksum field as parsed from the header, a snapshot taken at construction.
    /// On a locally built segment it holds the value computed in the constructor.
    /// </summary>
    public ushort Checksum { get; private set; }
    /// <summary>
    /// Gets the urgent pointer, a snapshot parsed from the header at construction.
    /// </summary>
    public ushort UrgentPointer { get; private set; }

    /// <summary>
    /// Gets the payload length in bytes: the length of the internet payload minus the TCP header length.
    /// </summary>
    public ushort TcpDataLength => (ushort)(Network.DataLength - TcpHeaderLength);

    /// <summary>
    /// Get TCP data.
    /// </summary>
    internal byte[] TcpData
    {
        get
        {
            byte[] data = new byte[TcpDataLength];
            RawData.AsSpan(DataOffset + TcpHeaderLength, data.Length).CopyTo(data);
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
        return $"TCP Packet {SourceIP}:{SourcePort} -> {DestinationIP}:{DestinationPort} (flags={GetFlags()}, seq={SequenceNumber}, ack={AckNumber})";
    }
}
