using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public enum ClassID
    {
        PCIDevice_2_0 = 0x00,
        MassStorageController = 0x01,
        NetworkController = 0x02,
        DisplayController = 0x03,
        MultimediaDevice = 0x04,
        MemoryController = 0x05,
        BridgeDevice = 0x06,
        SimpleCommController = 0x07,
        BaseSystemPreiph = 0x08,
        InputDevice = 0x09,
        DockingStations = 0x0A,
        Proccesors = 0x0B,
        SerialBusController = 0x0C,
        WirelessController = 0x0D,
        InteligentController = 0x0E,
        SateliteCommController = 0x0F,
        EncryptionController = 0x10,
        SignalProcessingController = 0x11,
        ProcessingAccelerators = 0x12,
        NonEssentialInstsrumentation = 0x13,
        Coprocessor = 0x40,
        Unclassified = 0xFF
    }
    
    public enum SubclassID
    {
        // MassStorageController: 
        SCSIStorageController = 0x00,
        IDEInterface = 0x01,
        FloppyDiskController = 0x02,
        IPIBusController = 0x03,
        RAIDController = 0x04,
        ATAController = 0x05,
        SATAController = 0x06,
        SASController = 0x07,
        NVMController = 0x08,
        UnknownMassStorage = 0x09,
    }
    
    public enum ProgramIF
    {
        // MassStorageController:
        SATA_VendorSpecific = 0x00,
        SATA_AHCI = 0x01,
        SATA_SerialStorageBus = 0x02,
        SAS_SerialStorageBus = 0x01,
        NVM_NVMHCI = 0x01,
        NVM_NVMExpress = 0x02
    }
    
    public enum VendorID
    {
        Intel = 0x8086,
        AMD = 0x0438,
        VMWare = 0x15AD
    }
    
    public enum DeviceID
    {
        SVGAIIAdapter = 0x0405
    }

    public class PCI
    {
        private static List<PCIDevice> Devices;

        public static uint Count
        {
            get { return (uint)Devices.Count; }
        }

        public static void Setup()
        {
            Devices = new List<PCIDevice>();
            if ((PCIDevice.GetHeaderType(0x0, 0x0, 0x0) & 0x80) == 0)
            {
                CheckBus(0x0);
            }
            else
            {
                for (ushort fn = 0; fn < 8; fn++)
                {
                    if (PCIDevice.GetVendorID(0x0, 0x0, fn) != 0xFFFF)
                        break;

                    CheckBus(fn);
                }
            }
        }

        private static void CheckBus(ushort xBus)
        {
            for (ushort device = 0; device < 32; device++)
            {
                if (PCIDevice.GetVendorID(xBus, device, 0x0) == 0xFFFF)
                    continue;

                CheckFunction(new PCIDevice(xBus, device, 0x0));
                if ((PCIDevice.GetHeaderType(xBus, device, 0x0) & 0x80) != 0)
                {
                    for (ushort fn = 1; fn < 8; fn++)
                    {
                        if (PCIDevice.GetVendorID(xBus, device, fn) != 0xFFFF)
                            CheckFunction(new PCIDevice(xBus, device, fn));
                    }
                }
            }
        }

        private static void CheckFunction(PCIDevice xPCIDevice)
        {
            Devices.Add(xPCIDevice);

            if (xPCIDevice.ClassCode == 0x6 && xPCIDevice.Subclass == 0x4)
                CheckBus(xPCIDevice.SecondaryBusNumber);
        }

        public static PCIDevice GetDevice(VendorID aVendorID, DeviceID aDeviceID)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                var xDevice = Devices[i];
                if ((VendorID)xDevice.VendorID == aVendorID &&
                    (DeviceID)xDevice.DeviceID == aDeviceID)
                {
                    return Devices[i];
                }
            }
            return null;
        }

        public static PCIDevice GetDeviceClass(ClassID Class, SubclassID SubClass)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                var xDevice = Devices[i];
                if ((ClassID)xDevice.ClassCode == Class &&
                    (SubclassID)xDevice.Subclass == SubClass)
                {
                    return Devices[i];
                }
            }
            return null;
        }

        public static PCIDevice GetDeviceClass(ClassID aClass, SubclassID aSubClass, ProgramIF aProgIF)
        {
            for (int i = 0; i < Devices.Count; i++)
            {
                var xDevice = Devices[i];
                if ((ClassID)xDevice.ClassCode == aClass &&
                    (SubclassID)xDevice.Subclass == aSubClass &&
                    (ProgramIF)xDevice.ProgIF == aProgIF)
                {
                    return Devices[i];
                }
            }
            return null;
        }
    }
}
