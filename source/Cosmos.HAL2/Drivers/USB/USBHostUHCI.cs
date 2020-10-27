﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL.USB
{
    public class USBHostUHCI : USBHost
    {
        public static PCIDevice pcis;
        public PCIBaseAddressBar bases;
        public static void ScanDevices()
        {
            for (var i = 0; i < PCI.Devices.Count; i++)
            {
                var pci = PCI.Devices[i];
                ///According to PCI Specs for USB
                ///0x00 = USB  (Universal Host Controller Spec) ,
                ///0x10 = USB (Open Host Controller Spec)
                ///0x20 = USB2 Host Controller (Intel Enhanced Host Controller Interface)
                ///0x30 = USB3 XHCI Controller
                if (pci.ClassCode == 0x0c)
                {
                    Console.WriteLine("ClassCode: " + pci.ClassCode + " SubClass: " + pci.Subclass + " ProgIF: " + pci.ProgIF);
                }
                if (pci.Subclass == 0x03)
                {
                    Console.WriteLine("ClassCode: " + pci.ClassCode + " SubClass: " + pci.Subclass + " ProgIF: " + pci.ProgIF);
                }
                if (pci.ClassCode == 0x0c &&
                    pci.Subclass == 0x03 &&
                    pci.ProgIF == 0x00)
                {
                    Console.WriteLine("Adding USB Device: " + pci.VendorID + " : " + pci.DeviceID);
                    Device.Add(new USBHostUHCI(pci));
                }
            }
        }

        private USBHostUHCIRegisters regs;
        public USBHostUHCI(PCIDevice pcidev)
        {

            regs = new USBHostUHCIRegisters(pcidev.BaseAddressBar[0]);

        }
    }
}
