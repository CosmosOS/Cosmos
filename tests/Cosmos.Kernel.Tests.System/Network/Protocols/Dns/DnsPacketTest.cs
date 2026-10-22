// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Protocols.Dns;
using Cosmos.Kernel.System.Network.Protocols.IPv4;
using Cosmos.Kernel.System.Network.Protocols.IPv6;
using NUnit.Framework;

// These cases are the tests of the experimental packet seam, so they opt into
// it rather than the project doing so for every other fixture.
#pragma warning disable COSMOS0002

namespace Cosmos.Kernel.Tests.System.Network.Protocols.Dns;

// DNS is one protocol at both IP versions, so one DnsPacket serves both and
// every query case runs over each. The answer frame comes from an independent
// Python implementation of RFC 1035, RFC 3596 and the RFC 8200 section 8.1
// checksum, not from the code under test.
[TestFixture]
public class DnsPacketTest
{
    private static readonly Address4 s_localV4 = new(10, 0, 2, 15);
    private static readonly Address4 s_serverV4 = new(1, 1, 1, 1);
    private static readonly Address6 s_localV6 = Address6.Parse("fe80::5054:ff:fe12:3456")!;
    private static readonly Address6 s_serverV6 = Address6.Parse("fe80::2")!;

    private const string Name = "example.com";

    // The question section starts 20 bytes into the transport payload: the
    // 8-byte UDP header then the 12-byte DNS header.
    private const int QuestionOffset = 20;

    // "example.com" as length-prefixed labels, then QTYPE and QCLASS 1 (IN).
    private static byte[] ExpectedQuestion(ushort recordType)
    {
        return
        [
            0x07, .. "example"u8, 0x03, .. "com"u8, 0x00,
            (byte)(recordType >> 8), (byte)recordType, 0x00, 0x01
        ];
    }

    private static byte[] QuestionOf(DnsPacketQuery packet, int length)
    {
        return packet.RawData.AsSpan(packet.Network.DataOffset + QuestionOffset, length).ToArray();
    }

    public class OverIPv4 : DnsPacketTest
    {
        [Test]
        public void GivenAQuery_WritesTheQuestionAndSettlesTheChecksum()
        {
            DnsPacketQuery packet = new(s_localV4, s_serverV4, Name);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network, Is.InstanceOf<IPPacket>());
                Assert.That(packet.Network.DataOffset, Is.EqualTo(34));
                Assert.That(packet.SourcePort, Is.EqualTo(53));
                Assert.That(packet.DestinationPort, Is.EqualTo(53));
                Assert.That(packet.Questions, Is.EqualTo(1));
                Assert.That(packet.AnswerRRs, Is.EqualTo(0));
                Assert.That(QuestionOf(packet, 17), Is.EqualTo(ExpectedQuestion(DnsRecordType.A)));
                Assert.That(packet.Checksum, Is.Not.EqualTo(0));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenAaaaIsAsked_TheQuestionCarriesTypeTwentyEight()
        {
            // Which record type a query asks for is independent of the version
            // carrying it: an IPv4 query can ask for an IPv6 address.
            DnsPacketQuery packet = new(s_localV4, s_serverV4, Name, DnsRecordType.AAAA);

            Assert.Multiple(() =>
            {
                Assert.That(QuestionOf(packet, 17), Is.EqualTo(ExpectedQuestion(DnsRecordType.AAAA)));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }
    }

    public class OverIPv6 : DnsPacketTest
    {
        [Test]
        public void GivenAQuery_SettlesTheChecksumThatIPv6Requires()
        {
            DnsPacketQuery packet = new(s_localV6, s_serverV6, Name);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network, Is.InstanceOf<IPv6Packet>());
                Assert.That(packet.Network.DataOffset, Is.EqualTo(54));
                Assert.That(QuestionOf(packet, 17), Is.EqualTo(ExpectedQuestion(DnsRecordType.A)));
                // The length-taking UDP constructor leaves the checksum at
                // zero for the caller to settle. Zero means "not computed"
                // over IPv4, but RFC 8200 section 8.1 forbids it over IPv6, so
                // a query that left it unset would be dropped by the server.
                Assert.That(packet.Checksum, Is.Not.EqualTo(0));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenAaaaIsAsked_TheQuestionCarriesTypeTwentyEight()
        {
            DnsPacketQuery packet = new(s_localV6, s_serverV6, Name, DnsRecordType.AAAA);

            Assert.Multiple(() =>
            {
                Assert.That(QuestionOf(packet, 17), Is.EqualTo(ExpectedQuestion(DnsRecordType.AAAA)));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }
    }

    public class Answers : DnsPacketTest
    {
        // A reply from fe80::2 carrying one AAAA record for example.com, with
        // its name as a compression pointer back into the question.
        private const string AaaaReply =
            "52540012345652540012349986dd600000000041" +
            "1140fe800000000000000000000000000002fe80" +
            "000000000000505400fffe123456003500350041" +
            "8ab3123481800001000100000000076578616d70" +
            "6c6503636f6d00001c0001c00c001c0001000001" +
            "2c001026062800022000010248189325c81946";

        [Test]
        public void GivenAnAaaaReplyOverIPv6_ParsesTheRecordAndItsAddress()
        {
            DnsPacketAnswer packet = new(Convert.FromHexString(AaaaReply));

            Assert.Multiple(() =>
            {
                // Python computed this checksum over the IPv6 pseudo-header,
                // so agreement here is a cross-implementation check.
                Assert.That(packet.VerifyChecksum(), Is.True);
                Assert.That(packet.Network.DataOffset, Is.EqualTo(54));
                Assert.That(packet.TransactionID, Is.EqualTo(0x1234));
                Assert.That(packet.Questions, Is.EqualTo(1));
                Assert.That(packet.AnswerRRs, Is.EqualTo(1));

                Assert.That(packet.Queries, Is.Not.Null);
                Assert.That(packet.Queries![0].Name, Is.EqualTo(Name));
                Assert.That(packet.Queries[0].Type, Is.EqualTo(DnsRecordType.AAAA));

                Assert.That(packet.Answers, Is.Not.Null);
                Assert.That(packet.Answers![0].Type, Is.EqualTo(DnsRecordType.AAAA));
                Assert.That(packet.Answers[0].TimeToLive, Is.EqualTo(300));
                Assert.That(packet.Answers[0].DataLength, Is.EqualTo(16));
                // The record name is a pointer, so resolving it proves the
                // compression offsets are read relative to the DNS header and
                // not to the start of the frame.
                Assert.That(packet.Answers[0].ResolvedName, Is.EqualTo(Name));
                Assert.That(packet.Answers[0].Address, Has.Length.EqualTo(16));
                Assert.That(new Address6(packet.Answers[0].Address!, 0),
                    Is.EqualTo(Address6.Parse("2606:2800:220:1:248:1893:25c8:1946")));
                Assert.That(packet.Truncated, Is.False);
            });
        }
    }

    // The frames below are written byte by byte here, over IPv4 from 10.0.2.3
    // to 10.0.2.15, with the UDP checksum left at zero (legal over IPv4, and
    // the parser does not verify it), so no code under test shapes them.
    public class MalformedAnswers : DnsPacketTest
    {
        // A record on its own: the name as in the question, type, class, TTL and data length.
        private static byte[] Question()
        {
            return [0x07, .. "example"u8, 0x03, .. "com"u8, 0x00, 0x00, 0x01, 0x00, 0x01];
        }

        private static byte[] Record(byte[] name, ushort type, byte[] data)
        {
            return
            [
                .. name,
                (byte)(type >> 8), (byte)type, 0x00, 0x01,
                0x00, 0x00, 0x01, 0x2c,
                (byte)(data.Length >> 8), (byte)data.Length,
                .. data
            ];
        }

        private static readonly byte[] s_pointerName = [0xc0, 0x0c];
        private static readonly byte[] s_inlineName = [0x07, .. "example"u8, 0x03, .. "com"u8, 0x00];

        // A DNS message with the given sections, and the UDP length claiming
        // udpLength bytes (the datagram's own length by default).
        private static byte[] Frame(ushort answers, byte[] sections, int? udpLength = null)
        {
            byte[] dns =
            [
                0x12, 0x34, 0x81, 0x80, 0x00, 0x01, (byte)(answers >> 8), (byte)answers, 0x00, 0x00, 0x00, 0x00,
                .. sections
            ];
            int udp = udpLength ?? dns.Length + 8;
            int ip = 20 + 8 + dns.Length;
            return
            [
                0x52, 0x54, 0x00, 0x12, 0x34, 0x56, 0x52, 0x54, 0x00, 0x12, 0x34, 0x03, 0x08, 0x00,
                0x45, 0x00, (byte)(ip >> 8), (byte)ip, 0x00, 0x00, 0x00, 0x00, 0x40, 0x11, 0x00, 0x00,
                10, 0, 2, 3, 10, 0, 2, 15,
                0x00, 0x35, 0x00, 0x35, (byte)(udp >> 8), (byte)udp, 0x00, 0x00,
                .. dns
            ];
        }

        [Test]
        public void GivenAnInlineRecordName_ReadsItAndTheRecordBehindIt()
        {
            // A resolver that rewrites a reply, as a caching stub does, may
            // spell a record's name out instead of pointing into the question.
            DnsPacketAnswer packet = new(Frame(1, [.. Question(), .. Record(s_inlineName, DnsRecordType.A, [93, 184, 216, 34])]));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.False);
                Assert.That(packet.Answers, Has.Count.EqualTo(1));
                Assert.That(packet.Answers![0].ResolvedName, Is.EqualTo(Name));
                Assert.That(packet.Answers[0].Type, Is.EqualTo(DnsRecordType.A));
                Assert.That(packet.Answers[0].TimeToLive, Is.EqualTo(300));
                Assert.That(packet.Answers[0].Address, Is.EqualTo(new byte[] { 93, 184, 216, 34 }));
            });
        }

        [Test]
        public void GivenInlineAndPointerNamesMixed_ReadsEveryRecord()
        {
            byte[] sections =
            [
                .. Question(),
                .. Record(s_inlineName, DnsRecordType.A, [93, 184, 216, 34]),
                .. Record(s_pointerName, DnsRecordType.A, [93, 184, 216, 35])
            ];
            DnsPacketAnswer packet = new(Frame(2, sections));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.False);
                Assert.That(packet.Answers, Has.Count.EqualTo(2));
                Assert.That(packet.Answers![0].ResolvedName, Is.EqualTo(Name));
                Assert.That(packet.Answers[1].ResolvedName, Is.EqualTo(Name));
                Assert.That(packet.Answers[1].Address, Is.EqualTo(new byte[] { 93, 184, 216, 35 }));
            });
        }

        [Test]
        public void GivenACnameWhoseTargetIsPartlyInline_ReadsTheWholeTarget()
        {
            // "www.example.com" as the label "www" then a pointer to the question's name.
            byte[] target = [0x03, .. "www"u8, 0xc0, 0x0c];
            byte[] sections =
            [
                .. Question(),
                .. Record(s_pointerName, DnsRecordType.CNAME, target),
                .. Record([0x03, .. "www"u8, 0xc0, 0x0c], DnsRecordType.A, [93, 184, 216, 34])
            ];
            DnsPacketAnswer packet = new(Frame(2, sections));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.False);
                Assert.That(packet.Answers, Has.Count.EqualTo(2));
                Assert.That(packet.Answers![0].CanonicalName, Is.EqualTo("www.example.com"));
                Assert.That(packet.Answers[1].ResolvedName, Is.EqualTo("www.example.com"));
            });
        }

        [Test]
        public void GivenMoreAnswersAnnouncedThanCarried_KeepsTheOnesCarriedAndReportsTheCut()
        {
            DnsPacketAnswer packet = new(Frame(3, [.. Question(), .. Record(s_pointerName, DnsRecordType.A, [93, 184, 216, 34])]));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Answers, Has.Count.EqualTo(1));
                Assert.That(packet.Answers![0].Address, Is.EqualTo(new byte[] { 93, 184, 216, 34 }));
            });
        }

        [Test]
        public void GivenADataLengthPastTheMessage_StopsBeforeIt()
        {
            byte[] record = Record(s_pointerName, DnsRecordType.A, [93, 184, 216, 34]);
            // Claim 0x0400 bytes of data for the four carried.
            record[^6] = 0x04;
            record[^5] = 0x00;
            DnsPacketAnswer packet = new(Frame(1, [.. Question(), .. record]));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Answers, Is.Empty);
            });
        }

        [Test]
        public void GivenALabelRunningPastTheMessage_StopsBeforeIt()
        {
            // A label of 40 bytes with 3 behind it.
            byte[] sections = [.. Question(), 0x28, .. "www"u8];
            DnsPacketAnswer packet = new(Frame(1, sections));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Queries, Has.Count.EqualTo(1));
                Assert.That(packet.Answers, Is.Empty);
            });
        }

        [Test]
        public void GivenAPointerLoop_StopsInsteadOfSpinning()
        {
            // An answer whose name points at itself.
            byte[] sections = [.. Question(), 0xc0, 0x1d, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x01, 0x2c, 0x00, 0x04, 93, 184, 216, 34];
            DnsPacketAnswer packet = new(Frame(1, sections));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Answers, Is.Empty);
            });
        }

        [Test]
        public void GivenAUdpLengthBeyondTheFrame_ReadsNoFurtherThanTheFrame()
        {
            // The whole record is announced, but the frame ends in its data.
            byte[] full = Frame(1, [.. Question(), .. Record(s_pointerName, DnsRecordType.A, [93, 184, 216, 34])]);
            DnsPacketAnswer packet = new(full[..^2]);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Answers, Is.Empty);
            });
        }

        [Test]
        public void GivenAUdpLengthShorterThanTheDnsHeader_ReportsTheCutWithoutSections()
        {
            DnsPacketAnswer packet = new(Frame(1, [.. Question(), .. Record(s_pointerName, DnsRecordType.A, [93, 184, 216, 34])], udpLength: 8 + 6));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Queries, Is.Null);
                Assert.That(packet.Answers, Is.Null);
            });
        }

        [Test]
        public void GivenACnameTargetRunningPastItsData_StopsBeforeIt()
        {
            // The target claims a 10-byte label where the record's data holds 4.
            byte[] sections = [.. Question(), .. Record(s_pointerName, DnsRecordType.CNAME, [0x0a, .. "www"u8])];
            DnsPacketAnswer packet = new(Frame(1, sections));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Truncated, Is.True);
                Assert.That(packet.Answers, Is.Empty);
            });
        }

        [Test]
        public void GivenAReservedLabelLength_StopsBeforeIt()
        {
            // 0x40 is one of the two label-length patterns RFC 1035 reserves.
            byte[] sections = [.. Question(), 0x40, .. "www"u8, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x01, 0x2c, 0x00, 0x04, 93, 184, 216, 34];
            DnsPacketAnswer packet = new(Frame(1, sections));

            Assert.That(packet.Truncated, Is.True);
        }
    }

    public class Addressing : DnsPacketTest
    {
        [Test]
        public void GivenTwoVersions_TheQueryConstructorRefusesThem()
        {
            Assert.Throws<ArgumentException>(() => _ = new DnsPacketQuery(s_localV4, s_serverV6, Name));
        }
    }
}
#pragma warning restore COSMOS0002
