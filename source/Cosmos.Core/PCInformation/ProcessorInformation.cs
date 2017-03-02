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
            uint[] returnValue;
            switch (operation)
            {
                case CPUIDOperation.GetVendorID:
                    returnValue = new uint[3];
                    returnValue[0] = GetCPUIDEBX(0);
                    returnValue[1] = GetCPUIDECX(0);
                    returnValue[2] = GetCPUIDEDX(0);
                    return returnValue;
                case CPUIDOperation.GetProcessorInformation:
                    //Returns the signature
                    returnValue = new uint[1];
                    returnValue[0] = GetCPUIDEAX(1);
                    return returnValue;
                case CPUIDOperation.GetFlags:
                    Debug.Kernel.Debugger.DoSend("Parse flags");
                    returnValue = new uint[2];
                    returnValue[0] = GetCPUIDECX(1);
                    returnValue[1] = GetCPUIDEDX(1);
                    return returnValue;
                default:
                    return null;
            }
        }

        [PlugMethod(PlugRequired = true)]
        public static int CanReadCPUID() => 0; //plugged

        //TODO Ideally, we would get the registers in one call. But i don't know how to do it
        /// <summary>
        /// Gets the cpuid eax register using the specified initial eax
        /// </summary>
        /// <remarks>This method is public only to be plugged. DON´T USE IT IN HAL</remarks>
        [PlugMethod(PlugRequired = true)]
        public static uint GetCPUIDEAX(uint eaxOperation) => 0; //Plugged
        /// <summary>
        /// Gets the cpuid ebx register using the specified initial eax
        /// </summary>
        /// <remarks>This method is public only to be plugged. DON´T USE IT IN HAL</remarks>
        [PlugMethod(PlugRequired = true)]
        public static uint GetCPUIDEBX(uint eaxOperation) => 0; //Plugged
        /// <summary>
        /// Gets the cpuid ecx register using the specified initial eax
        /// </summary>
        /// <remarks>This method is public only to be plugged. DON´T USE IT IN HAL</remarks>
        [PlugMethod(PlugRequired = true)]
        public static uint GetCPUIDECX(uint eaxOperation) => 0; //Plugged
        /// <summary>
        /// Gets the cpuid edx register using the specified initial eax
        /// </summary>
        /// <remarks>This method is public only to be plugged. DON´T USE IT IN HAL</remarks>
        [PlugMethod(PlugRequired = true)]
        public static uint GetCPUIDEDX(uint eaxOperation) => 0; //Plugged


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
