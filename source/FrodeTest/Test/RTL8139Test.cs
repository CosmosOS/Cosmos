using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.Devices.RTL8139;
using Cosmos.Sys.Network;

namespace FrodeTest.Test
{
    public class RTL8139Test
    {
        public static void RunTest()
        {
            // Testing RTL8139 PCI networkcard
            //Load card
            var xNic = RTL8139.FindAll()[0];

            xNic.Enable();
            xNic.InitializeDriver();

            byte[] xNetworkData = { (byte)'a' };
            //var xPingOut = new ICMPPacket(1, 2, ICMPPacket.ICMPType.EchoRequest, xPingData, 0);

            xNic.TransmitBytes(xNetworkData);
        }
    }
}
