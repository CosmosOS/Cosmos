using System;
using System.Collections.Generic;
using System.Text;

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
            Console.WriteLine("BaseAddress0 is : " + pciNic.BaseAddress0);
            Console.WriteLine("BaseAddress1 is : " + pciNic.BaseAddress1);
            Console.WriteLine("MAC address: " + nic.MACAddress.ToString());
            Console.WriteLine("Enabling card...");
            nic.Enable();
            //nic.SoftReset();
            nic.EnableRecieve();
            nic.EnableTransmit();

            
        }
    }
}
