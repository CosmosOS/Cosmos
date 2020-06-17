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
            HAL.Power.ACPIShutdown();
        }
    }
}
