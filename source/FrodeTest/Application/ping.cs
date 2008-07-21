using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Sys.Network;
using Cosmos.Hardware.Network.Devices.RTL8139;
using System.Net;

namespace FrodeTest.Application
{
    public class ping : IConsoleApplication
    {
        public int Execute(object args)
        {
            //Parse args into a host/ip
            //System.Net.IPAddress xDestinationIP = IPAddress.Parse("172.28.5.10");

            //Send ICMP Ping Request packet towards that host
            var xNic = RTL8139.FindAll()[0];
            xNic.Enable();
            xNic.InitializeDriver();

            byte[] xPingData = {(byte)'a', (byte)'b', (byte)'c'};
            var xPingOut = new ICMPPacket(1, 2, ICMPPacket.ICMPType.EchoRequest, xPingData, 0);

            if (xNic.TransmitBytes(xPingOut.GetData()))
                return 0;
            else
                return -1;
            
            //Wait for response back
        }

        public string CommandName
        {
            get { return "ping"; }
        }

        public string Description
        {
            get { return "Pings a destination with ICMP"; }
        }
    }
}
