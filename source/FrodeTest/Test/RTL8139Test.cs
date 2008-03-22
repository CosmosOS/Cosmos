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
            //Load card
            List<Cosmos.Driver.RTL8139.RTL8139> nics = Cosmos.Driver.RTL8139.RTL8139.FindRTL8139Devices();

            if (nics.Count == 0)
            {
                Console.WriteLine("No Realtek 8139 network card found!!");
                return;
            }

            Console.WriteLine(nics.Count + " network cards found");
            Cosmos.Driver.RTL8139.RTL8139 nic = (Cosmos.Driver.RTL8139.RTL8139)nics[0];

            Console.WriteLine("Network card: " + nic.Name);
            Console.WriteLine("HW Revision: " + nic.GetHardwareRevision());
            Console.WriteLine("MAC address: " + nic.MACAddress.ToString());

            //Console.WriteLine("BaseAddress0 is : " + pciNic.BaseAddress0);
            Console.WriteLine("BaseAddress1 is : " + nic.PCICard.BaseAddress1);
            Console.WriteLine("Enabling card...");
            //nic.SoftReset();
            nic.Enable();
            Console.WriteLine("Initializing driver...");
            nic.InitializeDriver();

            Cosmos.Driver.RTL8139.PacketHeader head = new Cosmos.Driver.RTL8139.PacketHeader(0xFF);
            byte[] data = Mock.FakeBroadcastPacket.GetFakePacketAllHigh();
            Cosmos.Driver.RTL8139.Packet packet = new Cosmos.Driver.RTL8139.Packet(head, data);
            nic.Transmit(packet);
        }
    }
}
