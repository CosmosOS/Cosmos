// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Text;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv6;
using Cosmos.Kernel.System.Network.UDP;
using NUnit.Framework;

// These cases are the tests of the experimental packet seam, so they opt into
// it rather than the project doing so for every other fixture.
#pragma warning disable COSMOS0002

namespace Cosmos.Kernel.Tests.System.Network.UDP;

// One UdpPacket class serves both IP versions, so every case here runs over
// each. Expected checksums come from an independent Python implementation of
// RFC 768 and RFC 8200 section 8.1 (standard library only), not from the code
// under test.
[TestFixture]
public class UdpPacketTest
{
    private static readonly Address4 s_localV4 = new(10, 0, 2, 15);
    private static readonly Address4 s_peerV4 = new(10, 0, 2, 2);
    private static readonly Address6 s_localV6 = Address6.Parse("fe80::5054:ff:fe12:3456")!;
    private static readonly Address6 s_peerV6 = Address6.Parse("fe80::2")!;

    private const ushort SourcePort = 5559;
    private const ushort DestinationPort = 5556;

    private static byte[] Payload() => Encoding.ASCII.GetBytes("COSMOS_SEAM_TEST");

    public class OverIPv4 : UdpPacketTest
    {
        [Test]
        public void GivenAPayload_WritesTheHeaderAndThePseudoHeaderChecksum()
        {
            UdpPacket packet = new(s_localV4, s_peerV4, SourcePort, DestinationPort, Payload());

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network, Is.InstanceOf<IPPacket>());
                Assert.That(packet.Network.EthernetType, Is.EqualTo(0x0800));
                Assert.That(packet.Network.DataOffset, Is.EqualTo(34));
                Assert.That(packet.Network.DataLength, Is.EqualTo(24));
                Assert.That(packet.SourceIP, Is.EqualTo(s_localV4));
                Assert.That(packet.DestinationIP, Is.EqualTo(s_peerV4));
                Assert.That(packet.SourcePort, Is.EqualTo(SourcePort));
                Assert.That(packet.DestinationPort, Is.EqualTo(DestinationPort));
                Assert.That(packet.UdpLength, Is.EqualTo(24));
                Assert.That(packet.UdpDataLength, Is.EqualTo(16));
                Assert.That(packet.Checksum, Is.EqualTo(0x3cc5));
                Assert.That(packet.VerifyChecksum(), Is.True);
                Assert.That(packet.GetUdpData(), Is.EqualTo(Payload()));
                Assert.That(packet.RawData, Has.Length.EqualTo(58));
            });
        }

        [Test]
        public void GivenTheProtocolNumber_TheCarryingPacketAnnouncesUdpWithoutDontFragment()
        {
            IPPacket network = (IPPacket)new UdpPacket(s_localV4, s_peerV4, SourcePort, DestinationPort, Payload()).Network;

            Assert.Multiple(() =>
            {
                Assert.That(network.Protocol, Is.EqualTo(17));
                Assert.That(network.IPFlags, Is.EqualTo(0));
            });
        }

        [Test]
        public void GivenALengthOnly_LeavesTheChecksumForTheCallerToWrite()
        {
            UdpPacket packet = new(s_localV4, s_peerV4, SourcePort, DestinationPort, (ushort)Payload().Length);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Checksum, Is.EqualTo(0));
                // Over IPv4 a zero checksum means the sender computed none,
                // which a receiver has to accept.
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenAPayloadWrittenAfterConstruction_WriteChecksumSettlesIt()
        {
            UdpPacket packet = new(s_localV4, s_peerV4, SourcePort, DestinationPort, (ushort)Payload().Length);
            Payload().CopyTo(packet.RawData.AsSpan(packet.Network.DataOffset + 8));

            packet.WriteChecksum();

            Assert.Multiple(() =>
            {
                Assert.That(packet.Checksum, Is.EqualTo(0x3cc5));
                Assert.That(packet.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenACorruptedPayload_TheChecksumDoesNotVerify()
        {
            UdpPacket packet = new(s_localV4, s_peerV4, SourcePort, DestinationPort, Payload());
            packet.RawData[^1] ^= 0x01;

            Assert.That(new UdpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }

        [Test]
        public void GivenACorruptedSourceAddress_TheChecksumDoesNotVerify()
        {
            UdpPacket packet = new(s_localV4, s_peerV4, SourcePort, DestinationPort, Payload());
            packet.RawData[26] ^= 0x01;

            Assert.That(new UdpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }
    }

    public class OverIPv6 : UdpPacketTest
    {
        [Test]
        public void GivenAPayload_WritesTheHeaderAndThePseudoHeaderChecksum()
        {
            UdpPacket packet = new(s_localV6, s_peerV6, SourcePort, DestinationPort, Payload());

            Assert.Multiple(() =>
            {
                Assert.That(packet.Network.EthernetType, Is.EqualTo(0x86DD));
                Assert.That(packet.Network.DataOffset, Is.EqualTo(54));
                Assert.That(packet.Network.DataLength, Is.EqualTo(24));
                Assert.That(packet.SourceIP, Is.EqualTo(s_localV6));
                Assert.That(packet.DestinationIP, Is.EqualTo(s_peerV6));
                Assert.That(packet.SourcePort, Is.EqualTo(SourcePort));
                Assert.That(packet.DestinationPort, Is.EqualTo(DestinationPort));
                Assert.That(packet.UdpLength, Is.EqualTo(24));
                Assert.That(packet.UdpDataLength, Is.EqualTo(16));
                Assert.That(packet.Checksum, Is.EqualTo(0xd415));
                Assert.That(packet.VerifyChecksum(), Is.True);
                Assert.That(packet.GetUdpData(), Is.EqualTo(Payload()));
                Assert.That(packet.RawData, Has.Length.EqualTo(78));
            });
        }

        [Test]
        public void GivenTheProtocolNumber_TheCarryingPacketAnnouncesUdpWithAHopLimit()
        {
            IPv6Packet network = (IPv6Packet)new UdpPacket(s_localV6, s_peerV6, SourcePort, DestinationPort, Payload()).Network;

            Assert.Multiple(() =>
            {
                Assert.That(network.NextHeader, Is.EqualTo(17));
                Assert.That(network.HopLimit, Is.EqualTo(64));
                Assert.That(network.PayloadLength, Is.EqualTo(24));
            });
        }

        [Test]
        public void GivenNoChecksum_TheDatagramIsCorrupt()
        {
            // RFC 8200 section 8.1 makes the UDP checksum mandatory over IPv6:
            // unlike IPv4, a zero field cannot mean "none computed".
            UdpPacket packet = new(s_localV6, s_peerV6, SourcePort, DestinationPort, (ushort)Payload().Length);

            Assert.Multiple(() =>
            {
                Assert.That(packet.Checksum, Is.EqualTo(0));
                Assert.That(packet.VerifyChecksum(), Is.False);
            });
        }

        [Test]
        public void GivenACorruptedPayload_TheChecksumDoesNotVerify()
        {
            UdpPacket packet = new(s_localV6, s_peerV6, SourcePort, DestinationPort, Payload());
            packet.RawData[^1] ^= 0x01;

            Assert.That(new UdpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }

        [Test]
        public void GivenACorruptedSourceAddress_TheChecksumDoesNotVerify()
        {
            UdpPacket packet = new(s_localV6, s_peerV6, SourcePort, DestinationPort, Payload());
            packet.RawData[IPv6Packet.SourceOffset + 15] ^= 0x01;

            Assert.That(new UdpPacket(packet.RawData).VerifyChecksum(), Is.False);
        }
    }

    public class Addressing : UdpPacketTest
    {
        [Test]
        public void GivenTwoVersions_TheBuildConstructorRefusesThem()
        {
            Assert.Throws<ArgumentException>(() =>
                _ = new UdpPacket(s_localV4, s_peerV6, SourcePort, DestinationPort, Payload()));
        }

        [Test]
        public void GivenAFrameThatCarriesNeitherVersion_TheParseConstructorRefusesIt()
        {
            byte[] arp = new byte[42];
            arp[12] = 0x08;
            arp[13] = 0x06;

            Assert.Throws<ArgumentException>(() => _ = new UdpPacket(arp));
        }
    }
}
#pragma warning restore COSMOS0002
