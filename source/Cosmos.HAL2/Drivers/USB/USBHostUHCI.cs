using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;
using Cosmos.Core;

namespace Cosmos.HAL.USB
{
    public class USBHostUHCI : USBHost
    {
        public static PCIDevice pcis;
        public PCIBaseAddressBar bases;
        public static bool USBDeviceFound = false;
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
                    pci.ProgIF == 0x00)
                {
                    Console.WriteLine("Adding USB Device: 0x" + pci.VendorID.ToHex() + " : 0x" + pci.DeviceID.ToHex());
                    USBDeviceFound = true;
                    Device.Add(new USBHostUHCI(pci));
                }
            }
        }

        private USBHostUHCIRegisters regs;
        public USBHostUHCI(PCIDevice pcidev)
        {

            regs = new USBHostUHCIRegisters(pcidev.BaseAddressBar[3]);

        }
    }
}
