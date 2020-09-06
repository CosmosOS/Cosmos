using Cosmos.Core;

namespace Cosmos.HAL
{
    /// <summary>
    /// Power class. Used to reboot and shutdown the PC.
    /// </summary>
    public class Power
    {
        //Reboot with CPU
        /// <summary>
        /// Reboot the CPU.
        /// </summary>
        public static void CPUReboot()
        {
            Core.Global.CPU.Reboot();
        }

        //Reboot with ACPI
        /// <summary>
        /// Reboot using ACPI.
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public static void ACPIReboot()
        {
            ACPI.Reboot(); //TODO
        }

        //Shutdown with ACPI
        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="System.IO.IOException">Thrown on IO error.</exception>
        public static void ACPIShutdown()
        {
            ACPI.Shutdown();
        }
    }
}
