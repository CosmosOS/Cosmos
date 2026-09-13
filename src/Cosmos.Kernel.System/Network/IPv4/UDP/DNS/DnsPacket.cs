/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network;

namespace Cosmos.Kernel.System.Network.IPv4.UDP.DNS;

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
/// DNS resource record types this stack understands (RFC 1035).
/// </summary>
internal static class DnsRecordType
{
    public const ushort A = 1;
    public const ushort CNAME = 5;
}

/// <summary>
/// A DNS question parsed from the question section of a response. Instances are parse products
/// created by <see cref="DnsPacketAnswer"/>, not builder inputs.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsQuery
{
    /// <summary>
    /// Parse products are produced by <see cref="DnsPacketAnswer"/>; every
    /// setter on this type is internal, so a caller-built instance could never
    /// be filled in.
    /// </summary>
    internal DnsQuery()
    {
    }

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
}

/// <summary>
/// A DNS resource record parsed from the answer section of a response. Instances are parse products
/// created by <see cref="DnsPacketAnswer"/>, not builder inputs.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsAnswer
{
    /// <summary>
    /// Parse products are produced by <see cref="DnsPacketAnswer"/>; every
    /// setter on this type is internal, so a caller-built instance could never
    /// be filled in.
    /// </summary>
    internal DnsAnswer()
    {
    }

    /// <summary>
    /// The raw 16-bit NAME field exactly as read from the record, usually a compression pointer
    /// (top two bits set) rather than a name.
    /// </summary>
    public ushort NameField { get; internal set; }

    /// <summary>
    /// The domain name obtained by following the compression pointer in <see cref="NameField"/>, or null
    /// when the NAME field is not a compression pointer (inline names are not resolved).
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
}

/// <summary>
/// Represents a DNS message carried over UDP (RFC 1035). The DNS header fields (transaction ID,
/// flags and section counts) are snapshots parsed from the raw buffer at construction and are never
/// recomputed.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsPacket : UdpPacket
{
    // Simple transaction ID generator
    private static byte s_transactionCounter = 1;

    /// <summary>
    /// Parses a DNS packet from a received frame. The buffer is aliased without copying, so later
    /// changes to <paramref name="rawData"/> are visible through the packet.
    /// </summary>
    /// <param name="rawData">The complete Ethernet frame containing the DNS message.</param>
    public DnsPacket(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Composes the UDP and DNS headers of a query between ports 53: a transaction ID taken from a
    /// static 8-bit counter (only the low byte is ever populated), flags 0x0100 (recursion desired),
    /// <paramref name="urlnb"/> questions and zero answer, authority and additional records. The
    /// question section itself is written by subclasses. Lengths and checksums are computed by the
    /// base constructors at construction and never recomputed.
    /// </summary>
    /// <param name="source">The source IPv4 address.</param>
    /// <param name="dest">The destination IPv4 address (the DNS server).</param>
    /// <param name="urlnb">The number of questions announced in the header.</param>
    /// <param name="len">The length in bytes of the DNS payload following the 12-byte DNS header.</param>
    public DnsPacket(Address source, Address dest, ushort urlnb, ushort len)
        : base(source, dest, 53, 53, (ushort)(len + 12))
    {
        byte transactionID = s_transactionCounter++;
        RawData[this.DataOffset + 8] = (byte)((transactionID >> 8) & 0xFF);
        RawData[this.DataOffset + 9] = (byte)((transactionID >> 0) & 0xFF);

        RawData[this.DataOffset + 10] = (byte)((0x0100 >> 8) & 0xFF);
        RawData[this.DataOffset + 11] = (byte)((0x0100 >> 0) & 0xFF);

        RawData[this.DataOffset + 12] = (byte)((urlnb >> 8) & 0xFF);
        RawData[this.DataOffset + 13] = (byte)((urlnb >> 0) & 0xFF);

        RawData[this.DataOffset + 14] = (byte)((0 >> 8) & 0xFF);
        RawData[this.DataOffset + 15] = (byte)((0 >> 0) & 0xFF);

        RawData[this.DataOffset + 16] = (byte)((0 >> 8) & 0xFF);
        RawData[this.DataOffset + 17] = (byte)((0 >> 0) & 0xFF);

        RawData[this.DataOffset + 18] = (byte)((0 >> 8) & 0xFF);
        RawData[this.DataOffset + 19] = (byte)((0 >> 0) & 0xFF);

        InitializeFields();
    }

    /// <summary>
    /// Parses the UDP fields, then captures the DNS header snapshot: transaction ID, flags and the
    /// question, answer, authority and additional record counts.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();
        TransactionID = (ushort)((RawData[this.DataOffset + 8] << 8) | RawData[this.DataOffset + 9]);
        DnsFlags = (ushort)((RawData[this.DataOffset + 10] << 8) | RawData[this.DataOffset + 11]);
        Questions = (ushort)((RawData[this.DataOffset + 12] << 8) | RawData[this.DataOffset + 13]);
        AnswerRRs = (ushort)((RawData[this.DataOffset + 14] << 8) | RawData[this.DataOffset + 15]);
        AuthorityRRs = (ushort)((RawData[this.DataOffset + 16] << 8) | RawData[this.DataOffset + 17]);
        AdditionalRRs = (ushort)((RawData[this.DataOffset + 18] << 8) | RawData[this.DataOffset + 19]);
    }

    /// <summary>
    /// Gets the domain name at the given offset. Does not follow compression
    /// pointers - use <see cref="ParseNameAt"/> for those.
    /// </summary>
    internal string ParseName(byte[] rawData, ref int index)
    {
        var url = new StringBuilder();

        while (rawData[index] != 0x00 && index < rawData.Length)
        {
            byte wordlength = rawData[index];
            index++;
            for (int j = 0; j < wordlength; j++)
            {
                url.Append((char)rawData[index]);
                index++;
            }
            url.Append('.');
        }

        index++; //End 0x00
        if (url.Length > 0)
        {
            return url.ToString().Substring(0, url.Length - 1);
        }
        return url.ToString();
    }

    /// <summary>
    /// Reads a domain name starting at <paramref name="startIndex"/>, following RFC 1035 compression
    /// pointers relative to <paramref name="messageBase"/>. Pointer chains are capped at 16 jumps to
    /// avoid loops.
    /// </summary>
    /// <param name="rawData">The buffer containing the DNS message.</param>
    /// <param name="startIndex">The index of the first name byte.</param>
    /// <param name="messageBase">The index of the first byte of the DNS header, used to resolve pointer offsets.</param>
    /// <returns>The dotted domain name without a trailing dot, or an empty string when no labels are present.</returns>
    private protected static string ParseNameAt(byte[] rawData, int startIndex, int messageBase)
    {
        StringBuilder sb = new();
        int pos = startIndex;
        int jumps = 0;
        // Avoid infinite pointer loops.
        const int maxJumps = 16;

        while (pos >= 0 && pos < rawData.Length)
        {
            byte b = rawData[pos];

            if (b == 0x00)
            {
                break;
            }

            if ((b & 0xC0) == 0xC0)
            {
                if (pos + 1 >= rawData.Length || jumps++ >= maxJumps)
                {
                    break;
                }

                int pointer = ((b & 0x3F) << 8) | rawData[pos + 1];
                pos = messageBase + pointer;
                continue;
            }

            pos++;
            for (int j = 0; j < b && pos < rawData.Length; j++, pos++)
            {
                sb.Append((char)rawData[pos]);
            }
            sb.Append('.');
        }

        if (sb.Length > 0)
        {
            // Trim trailing dot.
            sb.Length--;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Resolves a resource record's NAME field when it is a compression pointer.
    /// </summary>
    /// <param name="nameField">The raw 16-bit NAME field of the record.</param>
    /// <param name="rawData">The buffer containing the DNS message.</param>
    /// <param name="messageBase">The index of the first byte of the DNS header.</param>
    /// <returns>The decompressed name, or null when the field is not a compression pointer (inline names are not handled).</returns>
    private protected static string? ResolveRRName(ushort nameField, byte[] rawData, int messageBase)
    {
        if ((nameField & 0xC000) != 0xC000)
        {
            // Inline (non-compressed) RR names aren't handled.
            return null;
        }

        int offset = nameField & 0x3FFF;
        return ParseNameAt(rawData, messageBase + offset, messageBase);
    }

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

    /// <inheritdoc/>
    public override string ToString()
    {
        return "DNS Packet Src=" + SourceIP + ":" + SourcePort + ", Dest=" + DestinationIP + ":" + DestinationPort;
    }
}

/// <summary>
/// A DNS query packet that composes a single A/IN question for a given domain name.
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
    { }

    /// <summary>
    /// Composes a query with a single A/IN question for <paramref name="url"/>: the name is written
    /// as length-prefixed labels followed by QTYPE 1 (A) and QCLASS 1 (IN). Lengths and checksums
    /// are computed by the base constructors at construction and never recomputed.
    /// </summary>
    /// <param name="source">The source IPv4 address.</param>
    /// <param name="dest">The destination IPv4 address (the DNS server).</param>
    /// <param name="url">The domain name to resolve.</param>
    public DnsPacketQuery(Address source, Address dest, string url)
        : base(source, dest, 1, (ushort)(url.Length + url.Split('.').Length + 1 + 4))
    {
        int b = 0;

        foreach (string item in url.Split('.'))
        {
            byte[] word = Encoding.ASCII.GetBytes(item);

            RawData[this.DataOffset + 20 + b] = (byte)word.Length; //set word length

            b++;

            foreach (byte letter in word)
            {
                RawData[this.DataOffset + 20 + b] = letter;
                b++;
            }
        }

        RawData[this.DataOffset + 20 + b] = 0x00;

        RawData[this.DataOffset + 20 + b + 1] = 0x00;
        RawData[this.DataOffset + 20 + b + 2] = 0x01;

        RawData[this.DataOffset + 20 + b + 3] = 0x00;
        RawData[this.DataOffset + 20 + b + 4] = 0x01;
    }
}

/// <summary>
/// A DNS response packet. The full parse (header counts, question and answer sections, and
/// compression-pointer resolution) runs inside the constructor.
/// </summary>
[Experimental(Experimentals.PacketSeamDiagId)]
public class DnsPacketAnswer : DnsPacket
{
    /// <summary>
    /// Parses a DNS response from a received frame. The buffer is aliased without copying. The whole
    /// message is parsed during construction: header counts, the question and answer sections, and
    /// compression pointers; malformed input throws (typically <see cref="IndexOutOfRangeException"/>).
    /// When the reply code is not <see cref="ReplyCode.OK"/>, section parsing is skipped and
    /// <see cref="DnsPacket.Queries"/> and <see cref="DnsPacket.Answers"/> stay null.
    /// </summary>
    /// <param name="rawData">The complete Ethernet frame containing the DNS message.</param>
    public DnsPacketAnswer(byte[] rawData)
        : base(rawData)
    { }

    /// <summary>
    /// Parses the DNS header, then the question and answer sections, resolving compressed names.
    /// Skips section parsing when the reply code is not <see cref="ReplyCode.OK"/>.
    /// </summary>
    private protected override void InitializeFields()
    {
        base.InitializeFields();

        if ((ushort)(DnsFlags & 0x0F) != (ushort)ReplyCode.OK)
        {
            Serial.WriteString("[DNS] Packet response not OK. Passing packet.\n");
            return;
        }

        int index = DataOffset + 20;
        if (Questions > 0)
        {
            Queries = new List<DnsQuery>();

            for (int i = 0; i < Questions; i++)
            {
                var query = new DnsQuery();
                query.Name = ParseName(RawData, ref index);
                query.Type = (ushort)((RawData[index + 0] << 8) | RawData[index + 1]);
                query.Class = (ushort)((RawData[index + 2] << 8) | RawData[index + 3]);
                Queries.Add(query);
                index += 4;
            }
        }
        if (AnswerRRs > 0)
        {
            Answers = new List<DnsAnswer>();

            for (int i = 0; i < AnswerRRs; i++)
            {
                var answer = new DnsAnswer();
                answer.NameField = (ushort)((RawData[index + 0] << 8) | RawData[index + 1]);
                answer.ResolvedName = ResolveRRName(answer.NameField, RawData, DataOffset + 8);
                answer.Type = (ushort)((RawData[index + 2] << 8) | RawData[index + 3]);
                answer.Class = (ushort)((RawData[index + 4] << 8) | RawData[index + 5]);
                answer.TimeToLive = (RawData[index + 6] << 24) | (RawData[index + 7] << 16) | (RawData[index + 8] << 8) | RawData[index + 9];
                answer.DataLength = (ushort)((RawData[index + 10] << 8) | RawData[index + 11]);
                index += 12;

                int rdataStart = index;

                answer.Address = new byte[answer.DataLength];
                for (int j = 0; j < answer.DataLength; j++, index++)
                {
                    answer.Address[j] = RawData[index];
                }

                if (answer.Type == DnsRecordType.CNAME)
                {
                    answer.CanonicalName = ParseNameAt(RawData, rdataStart, DataOffset + 8);
                }

                Answers.Add(answer);
            }
        }
    }
}
