using Cosmos.Core;

namespace Cosmos.HAL
{
    public class Power
    {
        //Reboot with CPU
        public static void CPUReboot()
        {
            Core.Global.CPU.Reboot();
        }

        //Reboot with ACPI
        public static void ACPIReboot()
        {
            ACPI.Reboot(); //TODO
        }

        //Shutdown with ACPI
        public static void ACPIShutdown()
        {
            ACPI.Shutdown();
        }
    }
}
