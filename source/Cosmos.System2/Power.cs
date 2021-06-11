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
            // ACPI reset not done yet
            // ACPI.Reboot();

            Core.Global.CPU.Reboot();
        }

        /// <summary>
        /// Shutdown the ACPI.
        /// </summary>
        /// <exception cref="sysIO.IOException">Thrown on IO error.</exception>
        public static void Shutdown()
        {
            ACPI.Shutdown();
        }
    }
}
