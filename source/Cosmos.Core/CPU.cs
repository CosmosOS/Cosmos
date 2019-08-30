using System;
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

        public static string GetCPUVendorName()
        {
            if (CanReadCPUID() != 0)
            {
                // TODO Call cpuid and parse response
                int eax = 0;
                int ebx = 0;
                int ecx = 0;
                int edx = 0;
                ReadCPUID(0, ref eax, ref ebx, ref ecx, ref edx); // 0 is vendor name
                Global.mDebugger.Send("GetCPUVendorName Registers");
                Global.mDebugger.SendNumber(eax);
                Global.mDebugger.SendNumber(ebx);
                Global.mDebugger.SendNumber(ecx);
                Global.mDebugger.SendNumber(edx);
                return "";
            }

            throw new NotSupportedException();
        }

        public static ulong GetCPUUptime()
        {
            // TODO Divide by cpu clock speed
            return ReadTimestampCounter();
        }

        public static long GetCPUCycleSpeed()
        {
            if (CanReadCPUID() != 0)
            {
                // TODO read cpuid response and do a bitwise and 0x0000ffff
                int eax = 0;
                int ebx = 0;
                int ecx = 0;
                int edx = 0;
                ReadCPUID(16, ref eax, ref ebx, ref ecx, ref edx); // 16 is max cycle rate
                Global.mDebugger.Send("GetCPUCycleSpeed Registers");
                Global.mDebugger.SendNumber(eax);
                Global.mDebugger.SendNumber(ebx);
                Global.mDebugger.SendNumber(ecx);
                Global.mDebugger.SendNumber(edx);
                return 0;
            }

            throw new NotSupportedException();
        }

        internal static int CanReadCPUID() => throw new NotImplementedException();

        internal static void ReadCPUID(int type, ref int eax, ref int ebx, ref int ecx, ref int edx) => throw new NotImplementedException();

        internal static ulong ReadTimestampCounter() => throw new NotImplementedException();

        internal static int[] ReadFromModelSpecificRegister() => throw new NotImplementedException();
    }
}
