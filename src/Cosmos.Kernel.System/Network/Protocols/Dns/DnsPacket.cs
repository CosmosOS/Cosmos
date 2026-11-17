// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network.Protocols.Udp;

namespace Cosmos.Kernel.System.Network.Protocols.Dns;

/// <summary>
/// DNS reply codes (RCODE), carried in the lower four bits of the DNS header flags (RFC 1035).
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public enum ReplyCode
{
    /// <summary>
    /// No error condition.
    /// </summary>
    OK = 0,

    /// <summary>
    /// The server was unable to interpret the query.
    /// </summary>
    FormatError = 1,

    /// <summary>
    /// The server failed to process the query.
    /// </summary>
    ServerFailure = 2,

    /// <summary>
    /// The domain name referenced in the query does not exist (NXDOMAIN).
    /// </summary>
    NameError = 3,

    /// <summary>
    /// The server does not support the requested kind of query.
    /// </summary>
    NotSupported = 4,

    /// <summary>
    /// The server refuses to perform the requested operation.
    /// </summary>
    Refused = 5
}

/// <summary>
/// The DNS resource record types this stack understands: the address record of
/// each IP version, and the alias record that chains to them.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public static class DnsRecordType
{
    /// <summary>
    /// An IPv4 host address (RFC 1035), whose record data is four bytes.
    /// </summary>
    public const ushort A = 1;

    /// <summary>
    /// The canonical name behind an alias (RFC 1035), whose record data is a
    /// domain name that a lookup follows before it reaches an address record.
    /// </summary>
    public const ushort CNAME = 5;

    /// <summary>
    /// An IPv6 host address (RFC 3596), whose record data is sixteen bytes.
    /// </summary>
    public const ushort AAAA = 28;
}

/// <summary>
/// A DNS question parsed from the question section of a response. Instances are parse products
/// created by <see cref="DnsPacketAnswer"/>, not builder inputs.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsQuery
{
    /// <summary>
    /// The queried domain name, read label by label from the question section without following
    /// compression pointers, or <see langword="null"/> on a query this packet built rather than
    /// parsed: only the parse path fills it in.
    /// </summary>
    public string? Name { get; internal set; }

    /// <summary>
    /// The 16-bit question type (QTYPE), for example 1 for an A record.
    /// </summary>
    public ushort Type { get; internal set; }

    /// <summary>
    /// The 16-bit question class (QCLASS), 1 for the Internet class.
    /// </summary>
    public ushort Class { get; internal set; }

    /// <summary>
    /// Parse products are produced by <see cref="DnsPacketAnswer"/>; every
    /// setter on this type is internal, so a caller-built instance could never
    /// be filled in.
    /// </summary>
    internal DnsQuery()
    {
    }
}

/// <summary>
/// A DNS resource record parsed from the answer section of a response. Instances are parse products
/// created by <see cref="DnsPacketAnswer"/>, not builder inputs.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsAnswer
{
    /// <summary>
    /// The first two bytes of the record's NAME field exactly as read: the compression pointer
    /// when the name is one (top two bits set), the first label's length and first character
    /// when the name is written inline.
    /// </summary>
    public ushort NameField { get; internal set; }

    /// <summary>
    /// The record's owner name, decompressed when <see cref="NameField"/> is a pointer and
    /// read label by label when it is written inline.
    /// </summary>
    public string? ResolvedName { get; internal set; }

    /// <summary>
    /// The 16-bit record type (TYPE), for example 1 for A and 5 for CNAME.
    /// </summary>
    public ushort Type { get; internal set; }

    /// <summary>
    /// The 16-bit record class (CLASS), 1 for the Internet class.
    /// </summary>
    public ushort Class { get; internal set; }

    /// <summary>
    /// The record's time to live in seconds, read as a signed 32-bit value.
    /// </summary>
    public int TimeToLive { get; internal set; }

    /// <summary>
    /// The length in bytes of the record data (RDLENGTH).
    /// </summary>
    public ushort DataLength { get; internal set; }

    /// <summary>
    /// The raw RDATA bytes of the record: the four address bytes for an A record, but the encoded
    /// (possibly compressed) name bytes for a CNAME record. <see langword="null"/> on an answer
    /// this packet built rather than parsed: only the parse path fills it in.
    /// </summary>
    public byte[]? Address { get; internal set; }

    /// <summary>
    /// The decompressed CNAME target, set only for CNAME records; null for every other record type.
    /// </summary>
    public string? CanonicalName { get; internal set; }

    /// <summary>
    /// Parse products are produced by <see cref="DnsPacketAnswer"/>; every
    /// setter on this type is internal, so a caller-built instance could never
    /// be filled in.
    /// </summary>
    internal DnsAnswer()
    {
    }
}

/// <summary>
/// Represents a DNS message carried over UDP (RFC 1035). The DNS header fields (transaction ID,
/// flags and section counts) are snapshots parsed from the raw buffer at construction and are never
/// recomputed.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsPacket : UdpPacket
{
    /// <summary>
    /// The most compression pointers one name may follow, so a pointer loop in a malformed
    /// message ends instead of spinning.
    /// </summary>
    private const int MaxPointerJumps = 16;

    // A byte, so a built query only ever populates the low byte of the 16-bit transaction ID.
    private static byte s_transactionCounter = 1;

    /// <summary>
    /// The number of answer resource records announced in the header, parsed at construction.
    /// </summary>
    public ushort AnswerRRs { get; private set; }

    /// <summary>
    /// The number of authority resource records announced in the header, parsed at construction.
    /// </summary>
    public ushort AuthorityRRs { get; private set; }

    /// <summary>
    /// The number of additional resource records announced in the header, parsed at construction.
    /// </summary>
    public ushort AdditionalRRs { get; private set; }

    /// <summary>
    /// The 16-bit DNS transaction ID, parsed at construction. Packets built by this stack only ever
    /// populate the low byte, from a static 8-bit counter.
    /// </summary>
    public ushort TransactionID { get; private set; }

    /// <summary>
    /// The 16-bit DNS header flags, parsed at construction. The lower four bits of a response hold
    /// its <see cref="ReplyCode"/>.
    /// </summary>
    public ushort DnsFlags { get; private set; }

    /// <summary>
    /// The number of questions announced in the header, parsed at construction.
    /// </summary>
    public ushort Questions { get; private set; }

    /// <summary>
    /// The parsed question section. Populated only by the parsing subtypes
    /// (<see cref="DnsPacketAnswer"/>); null otherwise.
    /// </summary>
    public List<DnsQuery>? Queries { get; internal set; }

    /// <summary>
    /// The parsed answer section. Populated only by the parsing subtypes
    /// (<see cref="DnsPacketAnswer"/>); null otherwise.
    /// </summary>
    public List<DnsAnswer>? Answers { get; internal set; }

    /// <summary>
    /// Parses a DNS packet from a received frame. The buffer is aliased without copying, so later
    /// changes to <paramref name="rawData"/> are visible through the packet.
    /// </summary>
    /// <param name="rawData">The complete Ethernet frame containing the DNS message.</param>
    public DnsPacket(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Composes the UDP and DNS headers of a query between ports 53: a transaction ID taken from a
    /// static 8-bit counter (only the low byte is ever populated), flags 0x0100 (recursion desired),
    /// <paramref name="urlnb"/> questions and zero answer, authority and additional records. The
    /// question section itself is written by subclasses. Lengths and checksums are computed by the
    /// base constructors at construction and never recomputed.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address (the DNS server).</param>
    /// <param name="urlnb">The number of questions announced in the header.</param>
    /// <param name="len">The length in bytes of the DNS payload following the 12-byte DNS header.</param>
    public DnsPacket(Address source, Address dest, ushort urlnb, ushort len)
        : base(source, dest, 53, 53, (ushort)(len + 12))
    {
        byte transactionID = s_transactionCounter++;
        RawData[DataOffset + 8] = (byte)((transactionID >> 8) & 0xFF);
        RawData[DataOffset + 9] = (byte)((transactionID >> 0) & 0xFF);

        RawData[DataOffset + 10] = (byte)((0x0100 >> 8) & 0xFF);
        RawData[DataOffset + 11] = (byte)((0x0100 >> 0) & 0xFF);

        RawData[DataOffset + 12] = (byte)((urlnb >> 8) & 0xFF);
        RawData[DataOffset + 13] = (byte)((urlnb >> 0) & 0xFF);

        RawData[DataOffset + 14] = (byte)((0 >> 8) & 0xFF);
        RawData[DataOffset + 15] = (byte)((0 >> 0) & 0xFF);

        RawData[DataOffset + 16] = (byte)((0 >> 8) & 0xFF);
        RawData[DataOffset + 17] = (byte)((0 >> 0) & 0xFF);

        RawData[DataOffset + 18] = (byte)((0 >> 8) & 0xFF);
        RawData[DataOffset + 19] = (byte)((0 >> 0) & 0xFF);

        InitializeFields();
    }

    /// <summary>
    /// Parses the UDP fields, then captures the DNS header snapshot: transaction ID, flags and the
    /// question, answer, authority and additional record counts.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        TransactionID = (ushort)((RawData[DataOffset + 8] << 8) | RawData[DataOffset + 9]);
        DnsFlags = (ushort)((RawData[DataOffset + 10] << 8) | RawData[DataOffset + 11]);
        Questions = (ushort)((RawData[DataOffset + 12] << 8) | RawData[DataOffset + 13]);
        AnswerRRs = (ushort)((RawData[DataOffset + 14] << 8) | RawData[DataOffset + 15]);
        AuthorityRRs = (ushort)((RawData[DataOffset + 16] << 8) | RawData[DataOffset + 17]);
        AdditionalRRs = (ushort)((RawData[DataOffset + 18] << 8) | RawData[DataOffset + 19]);
    }

    /// <summary>
    /// Reads the domain name at <paramref name="index"/> and leaves <paramref name="index"/> on
    /// the byte after it, following RFC 1035 compression pointers relative to
    /// <paramref name="messageBase"/> for the name's text while counting only the bytes the
    /// name occupies in place: a pointer ends the name's two bytes there, wherever its target
    /// lies. Pointer chains are capped at <see cref="MaxPointerJumps"/> so a loop ends.
    /// </summary>
    /// <param name="rawData">The buffer containing the DNS message.</param>
    /// <param name="index">On entry the index of the first name byte; on return the index of
    /// the byte after the name, or <paramref name="end"/> when the name ran past it.</param>
    /// <param name="messageBase">The index of the first byte of the DNS header.</param>
    /// <param name="end">The index after the last byte of the DNS message.</param>
    /// <returns>The dotted domain name without a trailing dot, or <see langword="null"/> when
    /// the name runs past <paramref name="end"/>.</returns>
    private protected static string? ParseName(byte[] rawData, ref int index, int messageBase, int end)
    {
        StringBuilder name = new();
        int pos = index;
        int jumps = 0;

        // The index after the name in place, known once the first pointer is
        // met: the labels after it belong to the pointer's target.
        int after = -1;

        // A zero-length label closes the name. A label moves pos forward and a
        // pointer may move it back at most MaxPointerJumps times, so the loop
        // reaches the end of the message when no zero label closes the name.
        bool closed = false;
        while (pos < end && !closed)
        {
            byte lengthOrPointer = rawData[pos];

            if (lengthOrPointer == 0x00)
            {
                pos++;
                closed = true;
            }
            else if ((lengthOrPointer & 0xC0) == 0xC0)
            {
                if (pos + 1 >= end || jumps++ >= MaxPointerJumps)
                {
                    index = end;
                    return null;
                }

                if (after < 0)
                {
                    after = pos + 2;
                }

                pos = messageBase + (((lengthOrPointer & 0x3F) << 8) | rawData[pos + 1]);
            }
            else if ((lengthOrPointer & 0xC0) != 0 || pos + 1 + lengthOrPointer > end)
            {
                // The two spare bit patterns are reserved (RFC 1035 section 4.1.4).
                index = end;
                return null;
            }
            else
            {
                pos++;
                for (int j = 0; j < lengthOrPointer; j++, pos++)
                {
                    name.Append((char)rawData[pos]);
                }
                name.Append('.');
            }
        }

        if (!closed)
        {
            index = end;
            return null;
        }

        index = after < 0 ? pos : after;
        if (name.Length > 0)
        {
            // Trim trailing dot.
            name.Length--;
        }
        return name.ToString();
    }

    /// <summary>
    /// Reads the domain name at <paramref name="startIndex"/> without reporting where it ends;
    /// see <see cref="ParseName"/>.
    /// </summary>
    private protected static string? ParseNameAt(byte[] rawData, int startIndex, int messageBase, int end)
    {
        int index = startIndex;
        return ParseName(rawData, ref index, messageBase, end);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"DNS Packet Src={SourceIP}:{SourcePort}, Dest={DestinationIP}:{DestinationPort}";
    }
}

/// <summary>
/// A DNS query packet that composes a single IN-class question for a given domain name.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsPacketQuery : DnsPacket
{
    /// <summary>
    /// Parses a DNS query packet from a received frame. The buffer is aliased without copying.
    /// </summary>
    /// <param name="rawData">The complete Ethernet frame containing the DNS message.</param>
    public DnsPacketQuery(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Composes a query with a single IN-class question for <paramref name="url"/>: the name is
    /// written as length-prefixed labels followed by <paramref name="recordType"/> as the QTYPE and
    /// QCLASS 1 (IN). Lengths are computed by the base constructors, and the UDP checksum is
    /// settled here once the question section is in place, which IPv6 requires.
    /// </summary>
    /// <param name="source">The source address.</param>
    /// <param name="dest">The destination address (the DNS server).</param>
    /// <param name="url">The domain name to resolve.</param>
    /// <param name="recordType">The record type to ask for, one of <see cref="DnsRecordType"/>.
    /// Defaults to <see cref="DnsRecordType.A"/>. This is independent of the IP version carrying
    /// the query: either version can ask for either address record.</param>
    public DnsPacketQuery(Address source, Address dest, string url, ushort recordType = DnsRecordType.A)
        : base(source, dest, 1, (ushort)(url.Length + url.Split('.').Length + 1 + 4))
    {
        int offset = 0;

        foreach (string label in url.Split('.'))
        {
            byte[] labelBytes = Encoding.ASCII.GetBytes(label);

            RawData[DataOffset + 20 + offset] = (byte)labelBytes.Length;

            offset++;

            foreach (byte letter in labelBytes)
            {
                RawData[DataOffset + 20 + offset] = letter;
                offset++;
            }
        }

        RawData[DataOffset + 20 + offset] = 0x00;

        RawData[DataOffset + 20 + offset + 1] = (byte)((recordType >> 8) & 0xFF);
        RawData[DataOffset + 20 + offset + 2] = (byte)((recordType >> 0) & 0xFF);

        RawData[DataOffset + 20 + offset + 3] = 0x00;
        RawData[DataOffset + 20 + offset + 4] = 0x01;

        // The question section is only complete now, and the length-taking UDP
        // constructor leaves the checksum at zero. A zero is legal over IPv4,
        // where it means "not computed", but RFC 8200 section 8.1 makes it
        // illegal over IPv6, so an unsettled query would be dropped there.
        WriteChecksum();
    }
}

/// <summary>
/// A DNS response packet. The full parse (header counts, question and answer sections, and
/// compression-pointer resolution) runs inside the constructor.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsPacketAnswer : DnsPacket
{
    /// <summary>The bytes of a resource record between its name and its data: type, class, TTL and data length.</summary>
    private const int RecordFixedLength = 10;

    /// <summary>
    /// Parses a DNS response from a received frame. The buffer is aliased without copying. The whole
    /// message is parsed during construction: header counts, the question and answer sections, and
    /// the names, compressed or inline. The parse stops at the first malformed section, and
    /// <see cref="Truncated"/> reports it: <see cref="DnsPacket.Queries"/> then holds the
    /// questions read before it, and <see cref="DnsPacket.Answers"/> the answers. Nothing is read
    /// past the UDP length, and nothing throws. When the reply code is not
    /// <see cref="ReplyCode.OK"/>, section parsing is skipped and both lists stay null.
    /// </summary>
    /// <param name="rawData">The complete Ethernet frame containing the DNS message.</param>
    public DnsPacketAnswer(byte[] rawData)
        : base(rawData)
    {
    }

    /// <summary>
    /// Whether the parse stopped before the sections the header announced were complete: a
    /// message shorter than its UDP length, a name or a record running past the message's end,
    /// or a label with a reserved length. The sections read before the stop are kept.
    /// </summary>
    public bool Truncated { get; private set; }

    /// <summary>
    /// Parses the DNS header, then the question and answer sections, reading each name whether
    /// compressed or inline. Skips section parsing when the reply code is not
    /// <see cref="ReplyCode.OK"/>, and stops at the first malformed section (see
    /// <see cref="Truncated"/>).
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();

        if ((ushort)(DnsFlags & 0x0F) != (ushort)ReplyCode.OK)
        {
            Serial.WriteString("[DNS] Packet response not OK. Passing packet.\n");
            return;
        }

        // The message ends where the UDP length says, or where the buffer does if the
        // length claims more than arrived.
        int messageBase = DataOffset + UdpHeaderLength;
        int end = Math.Min(DataOffset + UdpLength, RawData.Length);
        if (end < messageBase + 12)
        {
            Truncated = true;
            return;
        }

        int index = messageBase + 12;
        if (Questions > 0)
        {
            Queries = [];

            for (int i = 0; i < Questions; i++)
            {
                string? name = ParseName(RawData, ref index, messageBase, end);
                if (name is null || index + 4 > end)
                {
                    Truncated = true;
                    return;
                }

                DnsQuery query = new()
                {
                    Name = name,
                    Type = (ushort)((RawData[index + 0] << 8) | RawData[index + 1]),
                    Class = (ushort)((RawData[index + 2] << 8) | RawData[index + 3])
                };
                Queries.Add(query);
                index += 4;
            }
        }

        if (AnswerRRs > 0)
        {
            Answers = [];

            for (int i = 0; i < AnswerRRs; i++)
            {
                if (index + 2 > end)
                {
                    Truncated = true;
                    return;
                }

                ushort nameField = (ushort)((RawData[index + 0] << 8) | RawData[index + 1]);
                string? name = ParseName(RawData, ref index, messageBase, end);
                if (name is null || index + RecordFixedLength > end)
                {
                    Truncated = true;
                    return;
                }

                DnsAnswer answer = new()
                {
                    NameField = nameField,
                    ResolvedName = name,
                    Type = (ushort)((RawData[index + 0] << 8) | RawData[index + 1]),
                    Class = (ushort)((RawData[index + 2] << 8) | RawData[index + 3]),
                    TimeToLive = (RawData[index + 4] << 24) | (RawData[index + 5] << 16) | (RawData[index + 6] << 8) | RawData[index + 7],
                    DataLength = (ushort)((RawData[index + 8] << 8) | RawData[index + 9])
                };
                index += RecordFixedLength;

                if (index + answer.DataLength > end)
                {
                    Truncated = true;
                    return;
                }

                answer.Address = RawData.AsSpan(index, answer.DataLength).ToArray();

                if (answer.Type == DnsRecordType.CNAME)
                {
                    // The target's labels lie in the record's data; its pointers may
                    // reach back anywhere before.
                    answer.CanonicalName = ParseNameAt(RawData, index, messageBase, index + answer.DataLength);
                    if (answer.CanonicalName is null)
                    {
                        Truncated = true;
                        return;
                    }
                }

                index += answer.DataLength;
                Answers.Add(answer);
            }
        }
    }
}
