using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.Network;

namespace FrodeTest.Test
{
    class ICMPv4Test
    {
        public static void RunTest()
        {

            //Create a ICMP packet
            ICMPPacket xICMP = new ICMPPacket(
                0xAC1C0606, //172.28.6.6
                0xAC1C050A, //172.28.5.10
                ICMPPacket.ICMPType.EchoRequest,
                new byte[] 
                { 
                    (byte)'a', 
                    (byte)'b', 
                    (byte)'c',
                    (byte)'d',
                    (byte)'e',
                    (byte)'f',
                    (byte)'g',
                    (byte)'h',
                    (byte)'i',
                    (byte)'j',
                    (byte)'k',
                    (byte)'l',
                    (byte)'m',
                    (byte)'n',
                    (byte)'o'
                },
                0x0 //Code
            );

            //Wrap ICMP packet inside EthernetPacket
            var xEthernet = new EthernetPacket(
                xICMP.GetData(), 
                0x525400123457, 0xFFFFFFFFFFFF
                , EthernetPacket.PacketType.IP);

            //Send EthernetPacket using RTL8139 network card
            var nic = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll()[0];
            nic.InitializeDriver();

            nic.TransmitBytes(xEthernet.GetData());
        }
    }
}
