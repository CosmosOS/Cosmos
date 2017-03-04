using System;
using Cosmos.IL2CPU.Plugs;
using Debugger = Cosmos.Debug.Kernel.Debugger;

namespace Cosmos.Core.PCInformation
{
    public unsafe class ProcessorInformation
    {
        /// <summary>
        /// Returns the Processor's vendor name
        /// </summary>
        /// <returns>CPU Vendor name</returns>

        /// <summary>
        /// Gets the information related to a certain cpuid operation
        /// The order of the registers returned is as follows (a register can be omited if it is not returned):
        /// EAX, EBX, ECX, EDX
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static uint[] GetCPUID(CPUIDOperation operation)
        {

            uint ptr = 0;
            uint eax;
            uint ebx;
            uint ecx;
            uint edx;
            uint[] returnValue;
            switch (operation)
            {
                case CPUIDOperation.GetVendorID:
                    returnValue = new uint[3];
                    CPUID(0, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = ebx;
                    returnValue[1] = ecx;
                    returnValue[2] = edx;
                    return returnValue;
                case CPUIDOperation.GetProcessorInformation:
                    //Returns the signature
                    returnValue = new uint[1];
                    CPUID(1, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = eax;
                    return returnValue;
                case CPUIDOperation.GetFlags:
                    Debug.Kernel.Debugger.DoSend("Parse flags");
                    returnValue = new uint[2];
                    CPUID(1, &eax, &ebx, &ecx, &edx);
                    returnValue[0] = ecx;
                    returnValue[1] = edx;
                    return returnValue;
                default:
                    return null;
            }
        }

        [PlugMethod(PlugRequired = true)]
        public static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx) { }

        [PlugMethod(PlugRequired = true)]
        public static int CanReadCPUID() => 0; //plugged

        [PlugMethod(PlugRequired = true)]
        public static int __maxrate() => 0;

        [PlugMethod(PlugRequired = true)]
        public static void __cyclesrdtsc(uint* rdtsc_hi, uint* rdtsc_lo) { }

        [PlugMethod(PlugRequired = true)]
        public static void __raterdmsr(uint* mperf_hi, uint* mperf_lo, uint* aperf_hi, uint* aperf_lo) { }

        /// <summary>
        /// Returns the number of CPU cycles since startup of the current CPU core
        /// </summary>
        /// <returns>Number of CPU cycles since startup</returns>
        public static long GetCycleCount() => 0; //plugged

        /// <summary>
        /// Returns the number of CPU cycles per seconds
        /// </summary>
        /// <returns>Number of CPU cycles per seconds</returns>
        public static long GetCycleRate() => 0; //plugged
    }
}
