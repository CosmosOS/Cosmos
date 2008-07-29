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
            var xNics = RTL8139.FindAll();

            if (xNics.Count == 0)
            {
                Console.WriteLine("No Realtek 8139 network card found!!");
                return;
            }

            Console.WriteLine(xNics.Count + " network cards found");
            var xNic = (RTL8139)xNics[0];

            xNic.Enable();
            xNic.InitializeDriver();

            byte[] xPingData = { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' };
            var xPingOut = new ICMPPacket(1, 2, ICMPPacket.ICMPType.EchoRequest, xPingData, 0);

            xNic.TransmitBytes(xPingOut.GetData());
        }
    }
}
