namespace Cosmos.System
{
    public static class Power
    {
        public static void Reboot()
        {
            HAL.Power.CPUReboot();
        }
        public static void Shutdown()
        {
            HAL.Power.ACPIShutdown();
        }
    }
}
