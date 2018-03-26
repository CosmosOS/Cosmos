using Cosmos.Core.PCInformation;
using Cosmos.Core_Asm.CPUInformationPlugs;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(ProcessorInformation))]
    public unsafe class ProcessorInformationImpl
    {
        /// <summary>
        /// Use the rdtsc instruction to read the current time stamp counter
        /// In edx will be stored the highest part of the rtdsc
        /// In eax the lowest part of the rtdsc
        /// </summary>
        /// <param name="edx">Lowest part of rtdsc</param>
        /// <param name="eax">Highest part of rtdsc</param>
        [PlugMethod(Assembler = typeof(GetCurrentTimeStampCounter))]
        public static void GetCurrentTimeStampCounter(uint* edx, uint* eax) { }

        /// <summary>
        /// This function queries cpuid to get the registers involved.
        /// If a value is not used it will contain garbage.
        /// Requires that none of the arguments are null. THIS IS PROGRAMMER RESPONSABILITY
        /// </summary>
        /// call example <c>CPUID(0, &eax, &ebx, &ecx, &edx);</c> where eax, ebx, and edx are UINT
        /// <param name="eaxOperation">Number of the operation that cpuid will do.</param>
        /// <param name="eax">returned eax register (not null)</param>
        /// <param name="ebx">returned ebx register (not null)</param>
        /// <param name="ecx">returned ecx register (not null)</param>
        /// <param name="edx">returned edx register (not null)</param>
        [PlugMethod(Assembler= typeof(CPUID))]
        public static void CPUID(uint eaxOperation, uint* eax, uint* ebx, uint* ecx, uint* edx) { }

        [PlugMethod(Assembler = typeof(CanReadCPUID))]
        public static int CanReadCPUID() { return 0; }

        /// <summary>
        /// The returned integer is like 0x8000000X so we need uint to avoid getting negative numbers (since its binary
        /// representation)
        /// </summary>
        /// <returns></returns>
        [PlugMethod(Assembler = typeof(GetHighestExtendedFunctionSupported))]
        public static uint GetHighestExtendedFunctionSupported() { return 0; }


        /// <summary>
        /// Read the model specific register using the rdmsr instruction.
        /// </summary>
        /// <param name="ecxOperation">ECX parameter passed to the rdmsr instruction</param>
        /// <param name="eax">Lower part of rdmsr</param>
        /// <param name="edx">Higher part of rdmsr</param>
        [PlugMethod(Assembler = typeof(RDMSR))]
        public static void RDMSR(uint ecxOperation, uint* eax, uint* edx) { }
    }
} 
