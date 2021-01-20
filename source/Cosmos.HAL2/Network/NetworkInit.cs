using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Network;

namespace Cosmos.HAL.Network
{
    public class NetworkInit
    {
        public static void Init()
        {
            int NetworkDeviceID = 0;

            Console.WriteLine("Searching for Ethernet Controllers...");

            foreach (PCIDevice device in PCI.Devices)
            {
                if ((device.ClassCode == 0x02) && (device.Subclass == 0x00) && // is Ethernet Controller
                    device == PCI.GetDevice(device.bus, device.slot, device.function))
                {

                    Console.WriteLine("Found " + PCIDevice.DeviceClass.GetDeviceString(device) + " on PCI " + device.bus + ":" + device.slot + ":" + device.function);

                    #region PCNETII

                    if (device.VendorID == (ushort)VendorID.AMD && device.DeviceID == (ushort)DeviceID.PCNETII)
                    {

                        Console.WriteLine("NIC IRQ: " + device.InterruptLine);

                        var AMDPCNetIIDevice = new AMDPCNetII(device);

                        AMDPCNetIIDevice.NameID = ("eth" + NetworkDeviceID);

                        Console.WriteLine("Registered at " + AMDPCNetIIDevice.NameID + " (" + AMDPCNetIIDevice.MACAddress.ToString() + ")");

                        AMDPCNetIIDevice.Enable();

                        NetworkDeviceID++;
                    }

                    #endregion

                }
            }

            if (NetworkDevice.Devices.Count == 0)
            {
                Console.WriteLine("No supported network card found!!");
            }
            else
            {
                Console.WriteLine("Network initialization done!");
            }
        }
    }
}
