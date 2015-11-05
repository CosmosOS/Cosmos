﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core
{
    public class PCI
    {
        private static List<PCIDevice> devices;

        public static void Setup()
        {
            EnumerateDevices();
        }

        public static PCIDevice GetDevice(ushort VendorID, ushort DeviceID)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].VendorID == VendorID && devices[i].DeviceID == DeviceID)
                    return devices[i];
            }
            return null;
        }

        private static void EnumerateDevices()
        {
            devices = new List<PCIDevice>();
            EnumerateBus(0, 0);
        }

        private static void EnumerateBus(uint xBus, uint step)
        {
            for (uint xDevice = 0; xDevice < 32; xDevice++)
            {
                PCIDevice xPCIDevice = new PCIDevice(xBus, xDevice, 0x00);
                if (xPCIDevice.DeviceExists)
                {
                    if (xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Bridge)
                    {
                        for (uint xFunction = 0; xFunction < 8; xFunction++)
                        {
                            xPCIDevice = new PCIDevice(xBus, xDevice, xFunction);
                            if (xPCIDevice.DeviceExists)
                                AddDevice(new PCIDeviceBridge(xBus, xDevice, xFunction), step);
                        }
                    }
                    else if (xPCIDevice.HeaderType == PCIDevice.PCIHeaderType.Cardbus)
                    {
                        AddDevice(new PCIDeviceCardbus(xBus, xDevice, 0x00), step);
                    }
                    else
                    {
                        AddDevice(new PCIDeviceNormal(xBus, xDevice, 0x00), step);
                    }
                }
            }
        }

        private static void AddDevice(PCIDevice device, uint step)
        {
            string str = "";
            for (int i = 0; i < step; i++)
                str += "     ";
            Console.WriteLine(str + device.bus + ":" + device.slot + ":" + device.function + "   " + PCIDevice.DeviceClass.GetString(device));
            devices.Add(device);
            if (device is PCIDeviceBridge)
                EnumerateBus(((PCIDeviceBridge)device).SecondaryBusNumber, step + 1);
        }
    }
}
