using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class RTL8139 {

        public static void Test() {
            Console.WriteLine("Start server application an another host,");
            Console.WriteLine("then press enter to send test packet.");
            Console.ReadLine();
            Console.WriteLine("Sending test packet");

            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                0x0A00020F // 10.0.2.15
                , 0x0449
                , 0xFFFFFFFF // 255.255.255.255, Broadcast
                , 2222
                , new byte[] { 0x16 });
            var xEthernet = new Cosmos.Sys.Network.EthernetPacket(xUDP.GetData()
                , 0x525400123457
                , 0xFFFFFFFFFFFF
                , 0x800);
            //xEthernet.SetSrcMAC(0x52, 0x54, 0x00, 0x12, 0x34, 0x57);

            var xNICs = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            var xNIC = xNICs[0];

            Console.WriteLine("Enabling network card!");
            Console.WriteLine(xNIC.Name);
            Console.WriteLine("Revision: " + xNIC.HardwareRevision);
            Console.WriteLine("MAC: " + xNIC.MACAddress);

            xNIC.Enable();
            xNIC.InitializeDriver();

            //Removed by Frode during cleanup
            xNIC.TransmitBytes(xEthernet.GetData());
        }

    }
}
