using Cosmos.Core;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Cosmos.System
{
    /// <summary>
    /// Manages the power state of the system.
    /// </summary>
    public static class Power
    {
        /// <summary>
        /// Reboots the system using the CPU.
        /// </summary>
        [DoesNotReturn]
        public static void Reboot()
        {
            /*
             * Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being too old.
             * This Provides a Reboot functionality.
            */
            if (VMTools.IsQEMU)
			{
                IOPort.Write8(0x64, 0xFE);
			}

            HAL.Power.CPUReboot();
        }

        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        [DoesNotReturn]
        public static void Shutdown()
        {
            /*
             * Detect if the VBOX guest service is present on the PCI bus.
             * https://forum.osdev.org/viewtopic.php?f=1&t=30674
             */
            if (VMTools.IsVirtualBox)
            {
                IOPort.Write32(0x4004, 0x3400);
            }

            /*
             * Qemu does not support ACPI at the current moment due to multiboot2 and SeaBios being to old.
             * This Provides a shutdown functionality.
            */
            if (VMTools.IsQEMU)
			{
                IOPort.Write16(0x604, 0x2000);
			}

            // Try Normal Method
            HAL.Power.ACPIShutdown();
        }
    }
}