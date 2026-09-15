// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv6;
using NUnit.Framework;

namespace Cosmos.Kernel.Tests.System.Network.IPv6;

// Expected values come from an independent Python implementation of RFC 4291
// and RFC 8200 section 8.1 (standard library only), not from the code under test.
[TestFixture]
public class Icmpv6PacketTest
{
    private static readonly MACAddress s_guestMac = new([0x52, 0x54, 0x00, 0x12, 0x34, 0x56]);
    private static readonly MACAddress s_peerMac = new([0x52, 0x54, 0x00, 0x12, 0x34, 0x99]);
    private static readonly Address6 s_guestLinkLocal = Address6.Parse("fe80::5054:ff:fe12:3456")!;
    private static readonly Address6 s_peerLinkLocal = Address6.Parse("fe80::2")!;
    private static readonly Address6 s_gateway = Address6.Parse("fec0::2")!;

    // Echo request from the guest to the peer: id 0x1234, sequence 1, the
    // 32-byte body holding its own offsets 8 to 39, checksum 0x7b26.
    private const string PeerFrame =
        "52540012349952540012345686dd6000000000283a40" +
        "fe80000000000000505400fffe123456" +
        "fe800000000000000000000000000002" +
        "80007b261234000108090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f2021222324252627";

    private static byte[] ExpectedBody() => Enumerable.Range(8, 32).Select(b => (byte)b).ToArray();

    public class EchoRequest : Icmpv6PacketTest
    {
        [Test]
        public void GivenSourceAndDestination_WritesHeaderAndPseudoHeaderChecksum()
        {
            Icmpv6EchoRequest request = new(s_guestLinkLocal, s_gateway, 0x1234, 1);

            Assert.Multiple(() =>
            {
                Assert.That(request.EthernetType, Is.EqualTo(0x86DD));
                Assert.That(request.PayloadLength, Is.EqualTo(40));
                Assert.That(request.NextHeader, Is.EqualTo(58));
                Assert.That(request.HopLimit, Is.EqualTo(64));
                Assert.That(request.SourceIP, Is.EqualTo(s_guestLinkLocal));
                Assert.That(request.DestinationIP, Is.EqualTo(s_gateway));
                Assert.That(request.IcmpType, Is.EqualTo(128));
                Assert.That(request.IcmpId, Is.EqualTo(0x1234));
                Assert.That(request.IcmpSequence, Is.EqualTo(1));
                Assert.That(request.IcmpChecksum, Is.EqualTo(0x7ae6));
                Assert.That(request.VerifyChecksum(), Is.True);
                Assert.That(request.GetIcmpData(), Is.EqualTo(ExpectedBody()));
            });
        }

        [Test]
        public void GivenFrameFromPeer_ParsesEveryField()
        {
            Icmpv6EchoRequest request = new(Convert.FromHexString(PeerFrame));

            Assert.Multiple(() =>
            {
                Assert.That(request.SourceMac, Is.EqualTo(s_guestMac));
                Assert.That(request.DestinationMac, Is.EqualTo(s_peerMac));
                Assert.That(request.SourceIP, Is.EqualTo(s_guestLinkLocal));
                Assert.That(request.DestinationIP, Is.EqualTo(s_peerLinkLocal));
                Assert.That(request.PayloadLength, Is.EqualTo(40));
                Assert.That(request.IcmpId, Is.EqualTo(0x1234));
                Assert.That(request.IcmpSequence, Is.EqualTo(1));
                Assert.That(request.IcmpChecksum, Is.EqualTo(0x7b26));
                Assert.That(request.VerifyChecksum(), Is.True);
                Assert.That(request.GetIcmpData(), Is.EqualTo(ExpectedBody()));
            });
        }

        [Test]
        public void GivenCorruptedBody_ChecksumDoesNotVerify()
        {
            byte[] frame = Convert.FromHexString(PeerFrame);
            frame[^1] ^= 0x01;

            Assert.That(new Icmpv6EchoRequest(frame).VerifyChecksum(), Is.False);
        }

        [Test]
        public void GivenCorruptedSourceAddress_ChecksumDoesNotVerify()
        {
            byte[] frame = Convert.FromHexString(PeerFrame);
            frame[IPv6Packet.SourceOffset + 15] ^= 0x01;

            Assert.That(new Icmpv6EchoRequest(frame).VerifyChecksum(), Is.False);
        }
    }

    public class EchoReply : Icmpv6PacketTest
    {
        [Test]
        public void GivenRequest_SwapsAddressesAndCopiesTheBody()
        {
            Icmpv6EchoRequest request = new(Convert.FromHexString(PeerFrame));

            Icmpv6EchoReply reply = new(request, s_peerMac);

            Assert.Multiple(() =>
            {
                Assert.That(reply.SourceIP, Is.EqualTo(s_peerLinkLocal));
                Assert.That(reply.DestinationIP, Is.EqualTo(s_guestLinkLocal));
                Assert.That(reply.SourceMac, Is.EqualTo(s_peerMac));
                Assert.That(reply.DestinationMac, Is.EqualTo(s_guestMac));
                Assert.That(reply.IcmpType, Is.EqualTo(129));
                Assert.That(reply.IcmpId, Is.EqualTo(0x1234));
                Assert.That(reply.IcmpSequence, Is.EqualTo(1));
                Assert.That(reply.PayloadLength, Is.EqualTo(40));
                Assert.That(reply.VerifyChecksum(), Is.True);
                Assert.That(reply.GetIcmpData(), Is.EqualTo(ExpectedBody()));
            });
        }
    }

    public class Solicitation : Icmpv6PacketTest
    {
        [Test]
        public void GivenTarget_AddressesItsSolicitedNodeGroupWithASourceLinkLayerOption()
        {
            NeighborSolicitation solicitation = new(s_guestLinkLocal, s_gateway, s_guestMac);

            Assert.Multiple(() =>
            {
                Assert.That(solicitation.SourceIP, Is.EqualTo(s_guestLinkLocal));
                Assert.That(solicitation.DestinationIP, Is.EqualTo(Address6.Parse("ff02::1:ff00:2")));
                Assert.That(solicitation.SourceMac, Is.EqualTo(s_guestMac));
                Assert.That(solicitation.DestinationMac, Is.EqualTo(new MACAddress([0x33, 0x33, 0xFF, 0x00, 0x00, 0x02])));
                Assert.That(solicitation.HopLimit, Is.EqualTo(255));
                Assert.That(solicitation.PayloadLength, Is.EqualTo(32));
                Assert.That(solicitation.IcmpType, Is.EqualTo(135));
                Assert.That(solicitation.Target, Is.EqualTo(s_gateway));
                Assert.That(solicitation.SourceLinkLayerAddress, Is.EqualTo(s_guestMac));
                Assert.That(solicitation.IcmpChecksum, Is.EqualTo(0x71e0));
                Assert.That(solicitation.VerifyChecksum(), Is.True);
            });
        }

        [Test]
        public void GivenNoOption_SourceLinkLayerAddressIsNull()
        {
            NeighborSolicitation built = new(s_guestLinkLocal, s_gateway, s_guestMac);
            byte[] frame = built.RawData.AsSpan(0, IPv6Packet.PayloadOffset + NdpPacket.MinimumLength).ToArray();
            frame[18] = 0;
            frame[19] = NdpPacket.MinimumLength;

            Assert.That(new NeighborSolicitation(frame).SourceLinkLayerAddress, Is.Null);
        }
    }

    public class Advertisement : Icmpv6PacketTest
    {
        [Test]
        public void GivenTarget_SetsSolicitedAndOverrideWithATargetLinkLayerOption()
        {
            NeighborAdvertisement advertisement = new(s_guestLinkLocal, s_peerLinkLocal, s_guestMac, s_peerMac, s_guestLinkLocal);

            Assert.Multiple(() =>
            {
                Assert.That(advertisement.SourceMac, Is.EqualTo(s_guestMac));
                Assert.That(advertisement.DestinationMac, Is.EqualTo(s_peerMac));
                Assert.That(advertisement.SourceIP, Is.EqualTo(s_guestLinkLocal));
                Assert.That(advertisement.DestinationIP, Is.EqualTo(s_peerLinkLocal));
                Assert.That(advertisement.HopLimit, Is.EqualTo(255));
                Assert.That(advertisement.IcmpType, Is.EqualTo(136));
                Assert.That(advertisement.RawData[IPv6Packet.PayloadOffset + 4], Is.EqualTo(0x60));
                Assert.That(advertisement.IsSolicited, Is.True);
                Assert.That(advertisement.Target, Is.EqualTo(s_guestLinkLocal));
                Assert.That(advertisement.TargetLinkLayerAddress, Is.EqualTo(s_guestMac));
                Assert.That(advertisement.IcmpChecksum, Is.EqualTo(0x8be9));
                Assert.That(advertisement.VerifyChecksum(), Is.True);
            });
        }
    }

    public class MulticastMac : Icmpv6PacketTest
    {
        [Test]
        public void GivenGroup_PrefixesThirtyThreeToItsLowFourBytes()
        {
            MACAddress actual = IPv6Packet.MulticastMac(Address6.Parse("ff02::1:ff12:3456")!);

            Assert.That(actual, Is.EqualTo(new MACAddress([0x33, 0x33, 0xFF, 0x12, 0x34, 0x56])));
        }
    }
}
