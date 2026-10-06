// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;
using Cosmos.Kernel.System.Network.TCP;
using NUnit.Framework;

// These cases are the tests of the experimental packet seam, so they opt into
// it rather than the project doing so for every other fixture.
#pragma warning disable COSMOS0002

namespace Cosmos.Kernel.Tests.System.Network.TCP;

// One TcpPacket class serves both IP versions, so every case here runs over
// each. Expected checksums come from an independent Python implementation of
// RFC 793 and RFC 8200 section 8.1 (standard library only), not from the code
// under test.
[TestFixture]
public class TcpPacketTest
{
    private static readonly Address4 s_localV4 = new(10, 0, 2, 15);
    private static readonly Address4 s_peerV4 = new(10, 0, 2, 2);
    private static readonly Address6 s_localV6 = Address6.Parse("fe80::5054:ff:fe12:3456")!;
    private static readonly Address6 s_peerV6 = Address6.Parse("fe80::2")!;

    private const ushort LocalPort = 49152;
    private const ushort PeerPort = 5557;

    private static byte[] Payload() => Encoding.ASCII.GetBytes("HELLO");

    public class OverIPv4 : TcpPacketTest
    {
        [Test]
        public void GivenAControlSegment_WritesTheHeaderAndThePseudoHeaderChecksum()
        {
            TcpPacket packet = new(s_localV4, s_peerV4, LocalPort, PeerPort,
                1000, 0, 20, (byte)TcpFlags.SYN, 8192, 0);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network, Is.InstanceOf<IPPacket>());
                Assert.That(packet.Network.DataOffset, Is.EqualTo(34));
                Assert.That(packet.Network.DataLength, Is.EqualTo(20));
                Assert.That(packet.SourceIP, Is.EqualTo(s_localV4));
                Assert.That(packet.DestinationIP, Is.EqualTo(s_peerV4));
                Assert.That(packet.SourcePort, Is.EqualTo(LocalPort));
                Assert.That(packet.DestinationPort, Is.EqualTo(PeerPort));
                Assert.That(packet.SequenceNumber, Is.EqualTo(1000));
                Assert.That(packet.AckNumber, Is.EqualTo(0));
                Assert.That(packet.TcpHeaderLength, Is.EqualTo(20));
                Assert.That(packet.TcpDataLength, Is.EqualTo(0));
                Assert.That(packet.WindowSize, Is.EqualTo(8192));
                Assert.That(packet.GetFlags(), Is.EqualTo("SYN"));
                Assert.That(packet.Checksum, Is.EqualTo(0x9e34));
                Assert.That(packet.VerifyChecksum(), Is.True);
                Assert.That(packet.Options, Is.Null);
            });
        }

        [Test]
        public void GivenAPayload_CopiesItInAndChecksumsTheWholeSegment()
        {
            TcpPacket packet = new(s_localV4, s_peerV4, LocalPort, PeerPort,
                2000, 3000, 20, (byte)(TcpFlags.PSH | TcpFlags.ACK), 8192, 0, Payload());

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network.DataLength, Is.EqualTo(25));
                Assert.That(packet.TcpDataLength, Is.EqualTo(5));
                Assert.That(packet.TcpData, Is.EqualTo(Payload()));
                Assert.That(packet.GetFlags(), Is.EqualTo("PSH|ACK"));
                Assert.That(packet.Checksum, Is.EqualTo(0xaae7));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenTheProtocolNumber_TheCarryingPacketAnnouncesTcpWithDontFragment()
        {
            IPPacket network = (IPPacket)new TcpPacket(s_localV4, s_peerV4, LocalPort, PeerPort,
                1000, 0, 20, (byte)TcpFlags.SYN, 8192, 0).Network;

            Assert.Multiple(() =>
            {
                Assert.That(network.Protocol, Is.EqualTo(6));
                // 0x40 in header byte 20 is Don't Fragment, which reads back as
                // flag bit 2 of 3.
                Assert.That(network.IPFlags, Is.EqualTo(2));
            });
        }

        [Test]
        public void GivenACorruptedPayload_TheChecksumDoesNotVerify()
        {
            TcpPacket packet = new(s_localV4, s_peerV4, LocalPort, PeerPort,
                2000, 3000, 20, (byte)(TcpFlags.PSH | TcpFlags.ACK), 8192, 0, Payload());
            packet.RawData[^1] ^= 0x01;

            Assert.That(new TcpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }
    }

    public class OverIPv6 : TcpPacketTest
    {
        [Test]
        public void GivenAControlSegment_WritesTheHeaderAndThePseudoHeaderChecksum()
        {
            TcpPacket packet = new(s_localV6, s_peerV6, LocalPort, PeerPort,
                1000, 0, 20, (byte)TcpFlags.SYN, 8192, 0);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network.EthernetType, Is.EqualTo(0x86DD));
                Assert.That(packet.Network.DataOffset, Is.EqualTo(54));
                Assert.That(packet.Network.DataLength, Is.EqualTo(20));
                Assert.That(packet.SourceIP, Is.EqualTo(s_localV6));
                Assert.That(packet.DestinationIP, Is.EqualTo(s_peerV6));
                Assert.That(packet.GetFlags(), Is.EqualTo("SYN"));
                Assert.That(packet.Checksum, Is.EqualTo(0x3585));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenAPayload_CopiesItInAndChecksumsTheWholeSegment()
        {
            TcpPacket packet = new(s_localV6, s_peerV6, LocalPort, PeerPort,
                2000, 3000, 20, (byte)(TcpFlags.PSH | TcpFlags.ACK), 8192, 0, Payload());

            Assert.Multiple(() =>
            {
                Assert.That(packet.TcpDataLength, Is.EqualTo(5));
                Assert.That(packet.TcpData, Is.EqualTo(Payload()));
                Assert.That(packet.Checksum, Is.EqualTo(0x4238));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenTheProtocolNumber_TheCarryingPacketAnnouncesTcpWithAHopLimit()
        {
            IPv6Packet network = (IPv6Packet)new TcpPacket(s_localV6, s_peerV6, LocalPort, PeerPort,
                1000, 0, 20, (byte)TcpFlags.SYN, 8192, 0).Network;

            Assert.Multiple(() =>
            {
                Assert.That(network.NextHeader, Is.EqualTo(6));
                Assert.That(network.HopLimit, Is.EqualTo(64));
            });
        }

        [Test]
        public void GivenACorruptedDestinationAddress_TheChecksumDoesNotVerify()
        {
            TcpPacket packet = new(s_localV6, s_peerV6, LocalPort, PeerPort,
                1000, 0, 20, (byte)TcpFlags.SYN, 8192, 0);
            packet.RawData[IPv6Packet.DestinationOffset + 15] ^= 0x01;

            Assert.That(new TcpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }
    }

    public class HeaderOptions : TcpPacketTest
    {
        // SYN|ACK from 10.0.2.2:5557 to 10.0.2.15:49152 behind an IPv4 header
        // with IHL 6: three NOPs and an end-of-options byte push the segment
        // from offset 34 to offset 38.
        private const string OptionedFrame =
            "52540012345652540012349908004600002c1234" +
            "000040064d870a0002020a00020f0101010015b5" +
            "c00000001388000003e9501220008a9b0000";

        [Test]
        public void GivenAnIPv4HeaderWithOptions_ReadsTheFlagsFromTheSegmentNotAFixedOffset()
        {
            TcpPacket packet = new(Convert.FromHexString(OptionedFrame));

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network.DataOffset, Is.EqualTo(38));
                Assert.That(packet.SourcePort, Is.EqualTo(PeerPort));
                Assert.That(packet.DestinationPort, Is.EqualTo(LocalPort));
                Assert.That(packet.SequenceNumber, Is.EqualTo(5000));
                Assert.That(packet.AckNumber, Is.EqualTo(1001));
                Assert.That(packet.FlagBits, Is.EqualTo((byte)(TcpFlags.SYN | TcpFlags.ACK)));
                // The flag bits used to be read from frame byte 47, which is
                // the flags byte only when the IPv4 header carries no options.
                // Here byte 47 sits inside the acknowledgment number and reads
                // zero, so every flag came back clear.
                Assert.That(packet.RawData[47], Is.EqualTo(0));
                Assert.That(packet.GetFlags(), Is.EqualTo("SYN|ACK"));
                Assert.That(packet.Checksum, Is.EqualTo(0x8a9b));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }
    }
}
#pragma warning restore COSMOS0002
