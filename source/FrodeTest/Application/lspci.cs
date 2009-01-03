using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Application
{
    class lspci : IConsoleApplication
    {
        public int Execute(string[] args)
        {
            foreach (Cosmos.Hardware.PCIDevice device in Cosmos.Hardware.PCIBus.Devices)
            {
                Console.WriteLine("[PCI] Vendor:\t" + device.VendorID + "\tRevision:\t" + device.RevisionID);
                Console.Write("\tStatus: " + device.Status);
                Console.Write("\tBus: "+ device.Bus);
                Console.WriteLine("\tSlot: " + device.Slot);
            }

            return 0;
        }

        public string CommandName
        {
            get { return "lspci"; }
        }

        public string Description
        {
            get { return "Lists all PCI devices"; }
        }
    }
}
