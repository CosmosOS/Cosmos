using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Driver.RTL8139.Register;

namespace FrodeTest.Test
{
    public class RTL8139Test
    {
        public static void RunTest()
        {
            // Testing RTL8139 PCI networkcard
            //Find PCI device
            Cosmos.Hardware.PC.Bus.PCIDevice pciNic = Cosmos.Hardware.PC.Bus.PCIDevice.GetPCIDevice(0, 3, 0);

            //Load card
            Cosmos.Driver.RTL8139.RTL8139 nic = new Cosmos.Driver.RTL8139.RTL8139(pciNic);

            Console.WriteLine("Network card: " + nic.Name);
            Console.WriteLine("HW Revision: " + nic.GetHardwareRevision());
            Console.WriteLine("MAC address: " + nic.MACAddress.ToString());

            Console.WriteLine("BaseAddress0 is : " + pciNic.BaseAddress0);
            Console.WriteLine("BaseAddress1 is : " + pciNic.BaseAddress1);
            Console.WriteLine("Enabling card...");
            nic.Enable();
            
            nic.SoftReset();
            nic.EnableRecieve();
            nic.EnableTransmit();
            Cosmos.Hardware.PC.Global.Sleep(50);
            Console.WriteLine("Timer: " + nic.TimerCount);

            Cosmos.Driver.RTL8139.PacketHeader head = new Cosmos.Driver.RTL8139.PacketHeader(0xFF);
            byte[] data = Mock.FakeBroadcastPacket.GetFakePacket();
            Cosmos.Driver.RTL8139.Packet packet = new Cosmos.Driver.RTL8139.Packet(head, data);
            nic.Transmit(packet);
            Console.WriteLine("Transmit called");
        }
    }
}
