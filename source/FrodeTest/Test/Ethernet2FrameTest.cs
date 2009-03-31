using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Hardware.Network.TCPIPModel.PhysicalLayer.Ethernet2;
using Cosmos.Hardware.Network;
using Cosmos.Hardware.Network.Devices.RTL8139;

namespace FrodeTest.Test
{
    public class Ethernet2FrameTest
    {
        public static void RunTest()
        {
            Ethernet2Frame frame = new Ethernet2Frame();
            byte[] dest = new byte[6];
            dest[0] = 11;
            dest[1] = 12;
            dest[2] = 13;
            dest[3] = 14;
            dest[4] = 15;
            dest[5] = 16;
            byte[] src = new byte[6];
            src[0] = 21;
            src[1] = 22;
            src[2] = 23;
            src[3] = 24;
            src[4] = 25;
            src[5] = 26;
            byte[] data = new byte[5];
            data[0] = 0xFF;
            data[1] = 0x0;
            data[2] = 0xFF;
            data[3] = 0xA;
            data[4] = 0xC;

            frame.Destination = new MACAddress(dest);
            frame.Source = new MACAddress(src);
            frame.Payload = data;

            NetworkDevice nic = Cosmos.Hardware.Network.NetworkDevice.NetworkDevices[0];
            if (nic == null)
                Console.WriteLine("No RTL8139C+ network card found");

            nic.Enable();
            nic.QueueBytes(frame.RawBytes());
        }
    }
}
