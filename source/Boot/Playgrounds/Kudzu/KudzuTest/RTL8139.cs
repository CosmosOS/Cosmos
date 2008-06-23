using System;
using System.Collections.Generic;
using System.Text;

namespace KudzuTest {
    public class RTL8139 {

        public static Frame CreateTestFrame() {
            var xFrame = new Frame();
            
            xFrame.InitTest();

            xFrame.SetEthSrcMAC(0x52, 0x54, 0x00, 0x12, 0x34, 0x57);
            xFrame.UpdateIPChecksum();
            xFrame.UpdateUDPChecksum();

            return xFrame;
        }

        public static void Test() {
            Console.WriteLine("Start server application an another host,");
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
