using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// An ICMP packet carried in an <see cref="IPPacket"/> (IP protocol 1).
/// Header properties are snapshots parsed from <see cref="EthernetPacket.RawData"/>
/// at construction time; the checksum is computed in the constructors and never
/// recomputed afterward.
/// </summary>
/// <remarks>
/// See also: <seealso cref="IPPacket"/>.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class IcmpPacket : IPPacket
{
    /// <summary>Parsed ICMP type backing <see cref="IcmpType"/>.</summary>
    private protected byte _icmpType;

    /// <summary>Parsed ICMP code backing <see cref="IcmpCode"/>.</summary>
    private protected byte _icmpCode;

    /// <summary>Parsed or computed ICMP checksum backing <see cref="IcmpCrc"/>.</summary>
    private protected ushort _icmpCRC;

    private static int s_echoRequestsReplied;
    private static byte[]? s_lastEchoRequestData;

    /// <summary>
    /// Number of echo requests answered with an echo reply.
    /// </summary>
    internal static int EchoRequestsReplied => s_echoRequestsReplied;

    /// <summary>
    /// ICMP payload of the most recently answered echo request.
    /// </summary>
    internal static byte[]? LastEchoRequestData => s_lastEchoRequestData;

    /// <summary>
    /// Handles an ICMP packet.
    /// </summary>
    /// <param name="packetData">The data of the packet.</param>
    internal static void ICMPHandler(byte[] packetData)
    {
        IcmpPacket icmpPacket = new(packetData);

        switch (icmpPacket.IcmpType)
        {
            case 0: // Echo reply
                Serial.WriteString("[ICMP] Received echo reply from ");
                Serial.WriteString(icmpPacket.SourceIP.ToString());
                Serial.WriteString("\n");

                IcmpClient? receiver = IcmpClient.GetClient(icmpPacket.SourceIP.Id);
                // Deliver the typed reply so consumers see the identifier
                // and sequence number, not just the base ICMP fields.
                receiver?.ReceiveData(new IcmpEchoReply(packetData));
                break;
            case 8: // Echo request
                IcmpEchoRequest request = new(packetData);
                IcmpEchoReply reply = new(request);

                Serial.WriteString("[ICMP] Sending echo reply to ");
                Serial.WriteString(reply.DestinationIP.ToString());
                Serial.WriteString("\n");

                OutgoingBuffer.AddPacket(reply);
                NetworkStack.Update();

                s_lastEchoRequestData = request.GetIcmpData();
                s_echoRequestsReplied++;
                break;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpPacket"/> class over
    /// existing frame bytes. The array is stored by reference, not copied:
    /// the caller must not reuse the buffer while the packet is alive.
    /// </summary>
    /// <param name="rawData">The raw data of the packet.</param>
    public IcmpPacket(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Parses the ICMP type, code, and checksum snapshots from
    /// <see cref="EthernetPacket.RawData"/>, in addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpType = RawData[DataOffset];
        _icmpCode = RawData[DataOffset + 1];
        _icmpCRC = (ushort)((RawData[DataOffset + 2] << 8) | RawData[DataOffset + 3]);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpPacket"/> class,
    /// building a frame with the given ICMP header values. The checksum is
    /// computed here, over the header and payload bytes as they are at this
    /// point, and is never recomputed: writes to the payload after
    /// construction invalidate it.
    /// </summary>
    /// <param name="source">Source IP address.</param>
    /// <param name="dest">Destination IP address.</param>
    /// <param name="type">ICMP type.</param>
    /// <param name="code">ICMP code.</param>
    /// <param name="id">ICMP identifier, written to the second header word.</param>
    /// <param name="seq">ICMP sequence number, written to the second header word.</param>
    /// <param name="icmpLength">The length in bytes of the ICMP header plus payload: the whole IP payload length.</param>
    public IcmpPacket(Address source, Address dest, byte type, byte code, ushort id, ushort seq, ushort icmpLength)
        : base(icmpLength, 1, source, dest, 0x00)
    {
        RawData[DataOffset] = type;
        RawData[DataOffset + 1] = code;
        RawData[DataOffset + 2] = 0x00;
        RawData[DataOffset + 3] = 0x00;
        RawData[DataOffset + 4] = (byte)((id >> 8) & 0xFF);
        RawData[DataOffset + 5] = (byte)((id >> 0) & 0xFF);
        RawData[DataOffset + 6] = (byte)((seq >> 8) & 0xFF);
        RawData[DataOffset + 7] = (byte)((seq >> 0) & 0xFF);

        _icmpCRC = CalcIcmpCrc(icmpLength);

        RawData[DataOffset + 2] = (byte)((_icmpCRC >> 8) & 0xFF);
        RawData[DataOffset + 3] = (byte)((_icmpCRC >> 0) & 0xFF);
        InitializeFields();
    }

    /// <summary>
    /// Computes the ones-complement checksum over the ICMP section of
    /// <see cref="EthernetPacket.RawData"/>, starting at the ICMP header. The
    /// result is not written to the buffer: constructors store it themselves,
    /// and nothing recomputes it after construction.
    /// </summary>
    /// <param name="length">The number of bytes to sum: the ICMP header plus payload.</param>
    /// <returns>The checksum value.</returns>
    private protected ushort CalcIcmpCrc(ushort length)
    {
        return CalcOcCrc(DataOffset, length);
    }

    /// <summary>
    /// The ICMP packet type, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public byte IcmpType => _icmpType;

    /// <summary>
    /// The ICMP packet code, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public byte IcmpCode => _icmpCode;

    /// <summary>
    /// The ICMP checksum, a snapshot taken at construction time. It is
    /// computed in the constructors and never recomputed afterward.
    /// </summary>
    public ushort IcmpCrc => _icmpCRC;

    /// <summary>
    /// The length in bytes of the ICMP payload: the IP payload length minus
    /// the 8-byte ICMP header.
    /// </summary>
    public ushort IcmpDataLength => (ushort)(DataLength - 8);

    /// <summary>
    /// Returns a fresh copy of the ICMP payload (the bytes after the 8-byte
    /// ICMP header). Mutating the returned array does not affect the packet.
    /// </summary>
    /// <returns>A new array holding the payload bytes.</returns>
    public byte[] GetIcmpData()
    {
        byte[] data = new byte[IcmpDataLength];

        for (int b = 0; b < IcmpDataLength; b++)
        {
            data[b] = RawData[DataOffset + 8 + b];
        }

        return data;
    }

    /// <summary>
    /// Returns a string describing the packet's source, destination, type, and code.
    /// </summary>
    /// <returns>The description string.</returns>
    public override string ToString()
    {
        return $"ICMP Packet Src={SourceIP}, Dest={DestinationIP}, Type={_icmpType}, Code={_icmpCode}";
    }
}

/// <summary>
/// An ICMP echo request packet. The identifier and sequence properties are
/// snapshots parsed from <see cref="EthernetPacket.RawData"/> at construction
/// time.
/// </summary>
/// <remarks>
/// See also: <seealso cref="IcmpPacket"/>.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class IcmpEchoRequest : IcmpPacket
{
    /// <summary>Parsed ICMP identifier backing <see cref="IcmpId"/>.</summary>
    private protected ushort _icmpID;

    /// <summary>Parsed ICMP sequence number backing <see cref="IcmpSequence"/>.</summary>
    private protected ushort _icmpSequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpEchoRequest"/> class
    /// over existing frame bytes. The array is stored by reference, not
    /// copied: the caller must not reuse the buffer while the packet is
    /// alive.
    /// </summary>
    /// <param name="rawData">The raw data of the packet.</param>
    public IcmpEchoRequest(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpEchoRequest"/> class,
    /// building a request with a 32-byte payload whose leading bytes are
    /// filled with an incrementing pattern. The checksum is computed here and
    /// never recomputed.
    /// </summary>
    /// <param name="source">Source IP address.</param>
    /// <param name="dest">Destination IP address.</param>
    /// <param name="id">ICMP echo identifier.</param>
    /// <param name="sequence">ICMP echo sequence number.</param>
    public IcmpEchoRequest(Address source, Address dest, ushort id, ushort sequence)
        : base(source, dest, 8, 0, id, sequence, 40)
    {
        for (int b = 8; b < IcmpDataLength; b++)
        {
            RawData[DataOffset + b] = (byte)b;
        }

        RawData[DataOffset + 2] = 0x00;
        RawData[DataOffset + 3] = 0x00;
        _icmpCRC = CalcIcmpCrc((ushort)(IcmpDataLength + 8));
        RawData[DataOffset + 2] = (byte)((_icmpCRC >> 8) & 0xFF);
        RawData[DataOffset + 3] = (byte)((_icmpCRC >> 0) & 0xFF);
    }

    /// <summary>
    /// Parses the identifier and sequence number snapshots from
    /// <see cref="EthernetPacket.RawData"/>, in addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
        _icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
    }

    /// <summary>
    /// The ICMP echo identifier, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public ushort IcmpId => _icmpID;

    /// <summary>
    /// The ICMP echo sequence number, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public ushort IcmpSequence => _icmpSequence;

    /// <summary>
    /// Returns a string describing the request's source, destination, identifier, and sequence number.
    /// </summary>
    /// <returns>The description string.</returns>
    public override string ToString()
    {
        return $"ICMP Echo Request Src={SourceIP}, Dest={DestinationIP}, ID={_icmpID}, Sequence={_icmpSequence}";
    }
}

/// <summary>
/// An ICMP echo reply packet. The identifier and sequence properties are
/// snapshots parsed from <see cref="EthernetPacket.RawData"/> at construction
/// time.
/// </summary>
/// <remarks>
/// See also: <seealso cref="IcmpPacket"/>.
/// </remarks>
[Experimental(Experimentals.PacketSeamDiagId)]
public class IcmpEchoReply : IcmpPacket
{
    /// <summary>Parsed ICMP identifier backing <see cref="IcmpId"/>.</summary>
    private protected ushort _icmpID;

    /// <summary>Parsed ICMP sequence number backing <see cref="IcmpSequence"/>.</summary>
    private protected ushort _icmpSequence;

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpEchoReply"/> class
    /// over existing frame bytes. The array is stored by reference, not
    /// copied: the caller must not reuse the buffer while the packet is
    /// alive.
    /// </summary>
    /// <param name="rawData">The raw data of the packet.</param>
    public IcmpEchoReply(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpEchoReply"/> class,
    /// building a reply to <paramref name="request"/>: source and destination
    /// are swapped, and the identifier, sequence number, and payload are
    /// copied from the request. The checksum is computed here and never
    /// recomputed.
    /// </summary>
    /// <param name="request">The ICMP echo request to answer.</param>
    public IcmpEchoReply(IcmpEchoRequest request)
        : base(request.DestinationIP, request.SourceIP, 0, 0, request.IcmpId, request.IcmpSequence, (ushort)(request.IcmpDataLength + 8))
    {
        for (int b = 0; b < IcmpDataLength; b++)
        {
            RawData[DataOffset + 8 + b] = request.RawData[DataOffset + 8 + b];
        }

        RawData[DataOffset + 2] = 0x00;
        RawData[DataOffset + 3] = 0x00;
        _icmpCRC = CalcIcmpCrc((ushort)(IcmpDataLength + 8));
        RawData[DataOffset + 2] = (byte)((_icmpCRC >> 8) & 0xFF);
        RawData[DataOffset + 3] = (byte)((_icmpCRC >> 0) & 0xFF);
    }

    /// <summary>
    /// Parses the identifier and sequence number snapshots from
    /// <see cref="EthernetPacket.RawData"/>, in addition to the base fields.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        _icmpID = (ushort)((RawData[DataOffset + 4] << 8) | RawData[DataOffset + 5]);
        _icmpSequence = (ushort)((RawData[DataOffset + 6] << 8) | RawData[DataOffset + 7]);
    }

    /// <summary>
    /// The ICMP echo identifier, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public ushort IcmpId => _icmpID;

    /// <summary>
    /// The ICMP echo sequence number, a snapshot parsed from <see cref="EthernetPacket.RawData"/> at construction time.
    /// </summary>
    public ushort IcmpSequence => _icmpSequence;

    /// <summary>
    /// Returns a string describing the reply's source, destination, identifier, and sequence number.
    /// </summary>
    /// <returns>The description string.</returns>
    public override string ToString()
    {
        return $"ICMP Echo Reply Src={SourceIP}, Dest={DestinationIP}, ID={_icmpID}, Sequence={_icmpSequence}";
    }
}
