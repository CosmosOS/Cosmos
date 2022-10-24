using Cosmos.Core;
using System;

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
            /*
             * Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being to old.
             * This Provides a Reboot functionality.
            */
            if (VMTools.IsQEMU)
			{
                new IOPort(0x64).Byte = 0xFE;
			}

            HAL.Power.CPUReboot();
        }

        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        public static void Shutdown()
        {
            /*
             * Detect if the VBOX guest service is present on the PCI bus.
             * https://forum.osdev.org/viewtopic.php?f=1&t=30674
             */
            if (VMTools.IsVirtualBox)
            {
                IOPort P = new(0x4004);
                P.DWord = 0x3400;
            }
            /*
             * Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being to old.
             * This Provides a shutdown functionality.
            */
            if (VMTools.IsQEMU)
			{
                new IOPort(0x604).Word = 0x2000;
			}

            // Try Normal Method
            HAL.Power.ACPIShutdown();
        }
    }
}
