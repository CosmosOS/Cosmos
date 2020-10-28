using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL.USB
{
    public abstract class USBHost : Device
    {
        public static void ScanDevices()
        {
            USBHostOHCI.ScanDevices();
            USBHostUHCI.ScanDevices();
        }
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
