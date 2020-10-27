using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.USB
{
    public class USBHostOHCI : USBHost
    {
        public static void ScanDevices()
        {
            foreach (PCIDevice pci in PCI.Devices)
            {
                ///According to PCI Specs for USB
                ///0x00 = USB  (Universal Host Controller Spec) ,
                ///0x10 = USB (Open Host Controller Spec)
                ///0x20 = USB2 Host Controller (Intel Enhanced Host Controller Interface)
                ///0x30 = USB3 XHCI Controller
                if (pci.ClassCode == 0x0c &&
                    pci.Subclass == 0x03 &&
                    pci.ProgIF == 0x10)
                {
                    Device.Add(new USBHostOHCI(pci));
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
