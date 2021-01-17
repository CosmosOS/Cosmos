using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Common.Extensions;

namespace Cosmos.HAL.USB
{
    public abstract class USBHost : Device
    {
        public USBHostOHCIRegisters uhci;
        public static void ScanDevices()
        {
            USBHostOHCI.ScanDevices();
            USBHostUHCI.ScanDevices();
        }
        public static void InitUSB(uint aProgIF = 0x10)
        {
            if(aProgIF == 0x10)
            {
                InitUHCI();
            }
            else if(aProgIF == 0x10)
            {
                initOHCI();
            }
            else if(aProgIF == 0x20)
            {
                initEHCI();
            }
            else if(aProgIF == 0x39)
            {
                initXHCI();
            }
        }
        public static void InitUHCI()
        {
            int i = 0;
            for (var i1 = 0; i1 < PCI.Devices.Count; i1++)
            {
                var pci = PCI.Devices[i1];
                ///According to PCI Specs for USB
                ///0x00 = USB  (Universal Host Controller Spec) ,
                ///0x10 = USB (Open Host Controller Spec)
                ///0x20 = USB2 Host Controller (Intel Enhanced Host Controller Interface)
                ///0x30 = USB3 XHCI Controller
                if (pci.ClassCode == 0x0c &&
                    pci.Subclass == 0x03 &&
                    pci.ProgIF == 0x00)
                {
                    i++;
                }
               
            }
            Console.WriteLine("Found: " + i.ToString() + " USB Root Hubs");
        }
        public static void initOHCI()
        {

        }
        public static void initEHCI()
        {
            throw new NotImplementedException();
        }
        public static void initXHCI()
        {
            throw new NotImplementedException();
        }
        #region UCHI
        public static void ScanUHCIUSB()
        {
            for (var i = 0; i < PCI.Devices.Count; i++)
            {
                var pci = PCI.Devices[i];
                if (pci.ClassCode == 0x0c && pci.Subclass == 0x03 && pci.ProgIF == 0x00)
                {
                    string uhcibase = pci.BaseAddressBar[4].BaseAddress.ToHex();
                    Console.WriteLine(uhcibase);
                }
            }
        }
        #endregion
        public static bool USBEnabled()
        {
            if (!USBHostUHCI.USBDeviceFound || USBHostOHCI.USBDeviceFound)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
