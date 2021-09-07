using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ApprovalTests;
using ApprovalTests.Reporters;

using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Tests
{
    [TestClass]
    public class ARPRequestTests
    {
        [TestMethod]
        [UseReporter(typeof(DiffReporter))]
        public void EnsureEncodedCorrectly()
        {
            var xMacSender = new MACAddress(
                new byte[]
                {
                    00,
                    0x50,
                    0x56,
                    0xc0,
                    0x00,
                    0x01
                });

            var xIPSender = new Address(192, 168, 17, 1);
            var xMacTarget = MACAddress.Broadcast;
            var xIPTarget = new Address(192, 168, 17, 102);
            var xArpTargetMac = new MACAddress(new byte[6]);

            var xPacket = new ARPRequest_Ethernet(xMacSender, xIPSender, xMacTarget, xIPTarget, xArpTargetMac);

            var xExpectedPacket =
                "FF FF FF FF FF FF 00 50 " + Environment.NewLine +
                "56 C0 00 01 08 06 00 01 " + Environment.NewLine +
                "08 00 06 04 00 01 00 50 " + Environment.NewLine +
                "56 C0 00 01 C0 A8 11 01 " + Environment.NewLine +
                "00 00 00 00 00 00 C0 A8 " + Environment.NewLine +
                "11 66";

            Approvals.Verify(Utilities.PrettyPrint(xPacket.RawData));
            Approvals.VerifyBinaryFile(xPacket.RawData, "bin");
            Assert.AreEqual(xExpectedPacket, Utilities.PrettyPrint(xPacket.RawData));
        }
    }
}
