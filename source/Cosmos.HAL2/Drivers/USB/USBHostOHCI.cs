using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL.USB
{
    public class USBHostOHCI : USBHost
    {
        public static void ScanDevices()
        {
            foreach (PCIDevice pci in PCIBus.Devices)
            {
                ///According to PCI Specs for USB
                ///0x00 = USB  (Universal Host Controller Spec) ,
                ///0x10 = USB (Open Host Controller Spec)
                ///0x20 = USB2 Host Controller (Intel Enhanced Host Controller Interface)
                ///0x30 = USB3 XHCI Controller
                if (pci.ClassCode == 0x0c && 
                    pci.SubClass == 0x03 && 
                    pci.ProgIF == 0x00 || pci.ProgIF == 0x10 || pci.ProgIF == 0x20 || pci.ProgIF == 0x30) 
                {
 
                    Device.Add(new USBHostOHCI(pci));
                }
            }
        }

        private PCIDeviceNormal mydevice;
        private USBHostOHCIRegisters regs;
        public USBHostOHCI(PCIDevice pcidev)
        {
            mydevice = pcidev as PCIDeviceNormal;
            // regs = new USBHostOHCIRegisters(pcidev.GetAddressSpace(0) as MemoryAddressSpace);
            regs = new USBHostOHCIRegisters(pcidev.GetAddressSpace(0) as MemoryAddressSpace);
        }


        public override string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}
