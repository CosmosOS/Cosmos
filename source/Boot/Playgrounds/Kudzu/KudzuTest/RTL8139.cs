using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Sys.Network;

namespace Cosmos.Playground.Kudzu {
    public class RTL8139 {

        public static void Test() {
            Console.WriteLine("Start server application an another host,");
            Console.WriteLine("then press enter to send test packet.");
            Console.ReadLine();
            Console.WriteLine("Sending test packet.");

            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                // Use a different port so it does not conflict wtih listener since we
                // are using the same IP on host for testing
                0x0A00020F, 32001 // 10.0.2.15
                , 0xFFFFFFFF, 32000 // 255.255.255.255, Broadcast
                , new byte[] { 0x16 });
            var xEthernet = new EthernetPacket(xUDP.GetData()
                , 0x525400123457, 0xFFFFFFFFFFFF
                , EthernetPacket.PacketType.IP);

            Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.DebugOutput = false;
            var xNICs = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            var xNIC = xNICs[0];

            Console.WriteLine(xNIC.Name);
            Console.WriteLine("Revision: " + xNIC.HardwareRevision);
            Console.WriteLine("MAC: " + xNIC.MACAddress);

            Console.WriteLine("Enabling network card.");
            xNIC.Enable();
            xNIC.InitializeDriver();

            Console.WriteLine("Sending bytes.");
            xNIC.TransmitBytes(xEthernet.GetData());
        }

    }
}
