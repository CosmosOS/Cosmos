using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class RTL8139 {

        public static Frame CreateTestFrame() {
            var xFrame = new Frame();
            xFrame.Init1();
            xFrame.SetEthSrcMAC(0x52, 0x54, 0x00, 0x12, 0x34, 0x57);
            xFrame.SetIPSrcAddr(10, 0, 2, 15);
            xFrame.SetIPDestAddr(255, 255, 255, 255);
            xFrame.UpdateIPChecksum();
            xFrame.UpdateUDPChecksum();
            //// Ethernet frame CRC - Done by card?

            //xFrame = new Frame();
            //xFrame.Init2();

            return xFrame;
        }

        public static void Test() {
            Console.WriteLine("Start listening application an another host,");
            Console.WriteLine("then press enter to send test packet.");
            Console.ReadLine();
            Console.WriteLine("Sending test packet");
            var xFrame = CreateTestFrame();

            var xNICs = Cosmos.Hardware.Network.Devices.RTL8139.RTL8139.FindAll();
            var xNIC = xNICs[0];

            Console.WriteLine("Enabling network card!");
            Console.WriteLine(xNIC.Name);
            Console.WriteLine("Revision: " + xNIC.HardwareRevision);
            Console.WriteLine("MAC: " + xNIC.MACAddress);

            xNIC.Enable();
            xNIC.InitializeDriver();

            xNIC.TransmitRaw(xFrame.mData);
        }

    }
}
