using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    // Non hardware class, only used by core and hardware drivers for ports etc.
    /// <summary>
    /// CPU class. Non hardware class, only used by core and hardware drivers for ports etc.
    /// </summary>
    public class CPU
    {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        /// <summary>
        /// Get amount of RAM in MB's. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static uint GetAmountOfRAM() => throw null;

        // needs to be static, as Heap needs it before we can instantiate objects
        /// <summary>
        /// Get end of the kernel. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static uint GetEndOfKernel() => throw null;

        /// <summary>
        /// Update IDT. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void UpdateIDT(bool aEnableInterruptsImmediately) => throw null;

        /// <summary>
        /// Init float. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void InitFloat() => throw null;

        /// <summary>
        /// Init SSE. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void InitSSE() => throw null;

        /// <summary>
        /// Zero fill. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public static void ZeroFill(uint aStartAddress, uint aLength) => throw null;

        /// <summary>
        /// Hult the CPU. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        public void Halt() => throw null;

        /// <summary>
        /// Reboot the CPU.
        /// </summary>
        public void Reboot()
        {
            // Disable all interrupts
            DisableInterrupts();

            var myPort = new IOPort(0x64);
            while ((myPort.Byte & 0x02) != 0)
            {
            }
            myPort.Byte = 0xFE;
            Halt(); // If it didn't work, Halt the CPU
        }

        /// <summary>
        /// Enable interrupts. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        private static void DoEnableInterrupts() => throw null;

        /// <summary>
        /// Disable interrupts. Plugged.
        /// </summary>
        [PlugMethod(PlugRequired = true)]
        private static void DoDisableInterrupts() => throw null;

        /// <summary>
        /// Check if interrupts enabled.
        /// </summary>
        [AsmMarker(AsmMarker.Type.Processor_IntsEnabled)]
        public static bool mInterruptsEnabled;

        /// <summary>
        /// Enable interrupts.
        /// </summary>
        public static void EnableInterrupts()
        {
            mInterruptsEnabled = true;
            DoEnableInterrupts();
        }

        /// <summary>
        /// Returns if the interrupts were actually enabled.
        /// </summary>
        /// <returns>bool value.</returns>
        public static bool DisableInterrupts()
        {
            DoDisableInterrupts();
            var xResult = mInterruptsEnabled;
            mInterruptsEnabled = false;
            return xResult;
        }
    }
}
