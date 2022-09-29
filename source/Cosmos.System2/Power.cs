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

            // Try Normal Method
            HAL.Power.ACPIShutdown();
        }

        /// <summary>
        /// Shutdown Qemu.
        /// Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being to old.
        /// This Provides a shutdown functionality.
        /// </summary>
        public static void QemuShutdown()
        {
            new IOPort(0x604).Word = 0x2000;
        }

        /// <summary>
        /// Reboot Qemu.
        /// Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being to old.
        /// This Provides a Reboot functionality.
        /// </summary>
        public static void QemuReboot()
        {
            new IOPort(0x64).Byte = 0xFE;
        }
    }
}
