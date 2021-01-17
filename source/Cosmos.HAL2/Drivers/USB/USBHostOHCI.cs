using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Cosmos.HAL.USB
{
    public class USBHostOHCI : USBHost
    {
        public static bool USBDeviceFound = false;
        public static void ScanDevices()
        {
            foreach (PCIDevice pci in PCI.Devices)
            {
                if (pci.ClassCode == 0x0c &&
                    pci.Subclass == 0x03 &&
                    pci.ProgIF == 0x10)
                {
                    Device.Add(new USBHostOHCI(pci));
                    USBDeviceFound = true;
                }
            }
        }
        private USBHostOHCIRegisters regs;
        public USBHostOHCI(PCIDevice pcidev)
        {

            regs = new USBHostOHCIRegisters(pcidev.BaseAddressBar[0]);

        }

    }
}
