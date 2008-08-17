using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Sys.Network;

namespace FrodeTest.Test
{
    public static class UDPTest
    {
        public static void RunTests() {
            var xUDP = new Cosmos.Sys.Network.UDPPacket(
                // Use a different port so it does not conflict wtih listener since we
                // are using the same IP on host for testing
                0x0A00020F, 32001 // 10.0.2.15
                , 0xFFFFFFFF, 32000 // 255.255.255.255, Broadcast
                , new byte[] { 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16 });
            var xEthernet = new EthernetPacket(xUDP.GetData()
                , 0x525400123457, 0xFFFFFFFFFFFF
                , EthernetPacket.PacketType.IP);

            Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.DebugOutput = false;
            var xNICs = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            var xNIC = xNICs[0];
            xNIC.Enable();
            xNIC.InitializeDriver();

            Console.WriteLine("Sending bytes.");
            var xBytes = xEthernet.GetData();
            System.Diagnostics.Debugger.Break();
            xNIC.TransmitBytes(xBytes);
        }
    }
}
