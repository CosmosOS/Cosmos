using Cosmos.Core;
using sysIO = System.IO;

namespace Cosmos.System
{
    /// <summary>
    /// Power class.
    /// </summary>
    public static class Power
    {
        /// <summary>
        /// Reboot with CPU.
        /// </summary>
        public static void Reboot()
        {
            // Try QEMU method
            new IOPort(0x64).Byte = 0xFE;
            
            // Try normal method
            HAL.Power.CPUReboot();
        }

        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
        public static void Shutdown()
        {
            /*
             * Detect if the VBOX guest service is present on the PCI bus.
             * https://forum.osdev.org/viewtopic.php?f=1&t=30674
             */
            if (HAL.PCI.Exists((HAL.VendorID)0x80EE, (HAL.DeviceID)0xCAFE))
            {
                IOPort P = new(0x4004);
                P.DWord = 0x3400;
            }
            // Try QEMU method
            new IOPort(0x604).Word = 0x2000;
            
            // Try Normal Method
            HAL.Power.ACPIShutdown();
        }
    }
}
