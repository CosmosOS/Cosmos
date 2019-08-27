using Cosmos.Core;

using IL2CPU.API.Attribs;

namespace Cosmos.Core_Asm
{
    [Plug(Target = typeof(CPU))]
    public class CPUImpl
    {
        [PlugMethod(Assembler = typeof(CPUUpdateIDTAsm))]
        public static void UpdateIDT(CPU aThis, bool aEnableInterruptsImmediately) => throw null;

        [PlugMethod(Assembler = typeof(CPUGetAmountOfRAMAsm))]
        public static uint GetAmountOfRAM() => throw null;

        [PlugMethod(Assembler = typeof(CPUGetEndOfKernelAsm))]
        public static uint GetEndOfKernel() => throw null;

        [PlugMethod(Assembler = typeof(CPUZeroFillAsm))]
        // TODO: implement this using REP STOSB and REPO STOSD
        public static void ZeroFill(uint aStartAddress, uint aLength) => throw null;

        [PlugMethod(Assembler = typeof(CPUInitFloatAsm))]
        public static void InitFloat(CPU aThis) => throw null;

        [PlugMethod(Assembler = typeof(CPUInitSSEAsm))]
        public static void InitSSE(CPU aThis) => throw null;

        [PlugMethod(Assembler = typeof(CPUHaltAsm))]
        public static void Halt(CPU aThis) => throw null;

        [PlugMethod(Assembler = typeof(CPUDisableINTsAsm))]
        public static void DoDisableInterrupts() => throw null;

        [PlugMethod(Assembler = typeof(CPUEnableINTsAsm))]
        public static void DoEnableInterrupts() => throw null;
    }
}
