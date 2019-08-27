using IL2CPU.API.Attribs;

namespace Cosmos.Core
{
    // Non hardware class, only used by core and hardware drivers for ports etc.
    public class CPU
    {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        [PlugMethod(PlugRequired = true)]
        public static uint GetAmountOfRAM() => throw null;

        // needs to be static, as Heap needs it before we can instantiate objects
        [PlugMethod(PlugRequired = true)]
        public static uint GetEndOfKernel() => throw null;

        [PlugMethod(PlugRequired = true)]
        public void UpdateIDT(bool aEnableInterruptsImmediately) => throw null;

        [PlugMethod(PlugRequired = true)]
        public void InitFloat() => throw null;

        [PlugMethod(PlugRequired = true)]
        public void InitSSE() => throw null;

        [PlugMethod(PlugRequired = true)]
        public static void ZeroFill(uint aStartAddress, uint aLength) => throw null;

        [PlugMethod(PlugRequired = true)]
        public void Halt() => throw null;

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

        [PlugMethod(PlugRequired = true)]
        private static void DoEnableInterrupts() => throw null;

        [PlugMethod(PlugRequired = true)]
        private static void DoDisableInterrupts() => throw null;

        [AsmMarker(AsmMarker.Type.Processor_IntsEnabled)]
        public static bool mInterruptsEnabled;

        public static void EnableInterrupts()
        {
            mInterruptsEnabled = true;
            DoEnableInterrupts();
        }

        /// <summary>
        /// Returns if the interrupts were actually enabled
        /// </summary>
        /// <returns></returns>
        public static bool DisableInterrupts()
        {
            DoDisableInterrupts();
            var xResult = mInterruptsEnabled;
            mInterruptsEnabled = false;
            return xResult;
        }
    }
}
