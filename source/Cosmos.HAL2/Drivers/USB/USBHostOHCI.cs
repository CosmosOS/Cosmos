//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Cosmos.Kernel;

//namespace Cosmos.HAL.Drivers.USB
//{
//    public class USBHostOHCI : USBHost
//    {
//        public static void ScanDevices()
//        {
//            foreach (PCIDevice pci in PCIBus.Devices)
//            {
//                if (pci.ClassCode == 0x0c && //bus
//                    pci.SubClass == 0x03 && //usb
//                    pci.ProgIF == 0x10) //ohci :D 
//                {
//                    //(as this is an open standard, vendor/device specific implementations should all work the same)
//                    Device.Add(new USBHostOHCI(pci));
//                }
//            }
//        }

//        private PCIDeviceNormal mydevice;
//        private USBHostOHCIRegisters regs;
//        public USBHostOHCI(PCIDevice pcidev)
//        {
//            mydevice = pcidev as PCIDeviceNormal;
//            regs = new USBHostOHCIRegisters(pcidev.GetAddressSpace(0) as MemoryAddressSpace);
//        }


//        public override string Name
//        {
//            get { throw new NotImplementedException(); }
//        }
//    }
//}
