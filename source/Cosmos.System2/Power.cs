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
        /// Shutdown with ACPI.
        /// </summary>
        public static void Shutdown()
        {
            HAL.Power.ACPIShutdown();
        }
    }
}
