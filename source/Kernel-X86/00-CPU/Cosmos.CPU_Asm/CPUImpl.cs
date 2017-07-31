using Cosmos.IL2CPU.API;

namespace Cosmos.CPU_Asm {
    [Plug(Target = typeof(Cosmos.CPU.Processor))]
    public class CPUImpl {
        [PlugMethod(Assembler = typeof(CPUUpdateIDTAsm))]
        public static void UpdateIDT(Cosmos.CPU.Processor aThis, bool aEnableInterruptsImmediately) {
        }

        [PlugMethod(Assembler = typeof(CPUGetAmountOfRAMAsm))]
        public static uint GetAmountOfRAM() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(CPUGetEndOfKernelAsm))]
        public static uint GetEndOfKernel() {
            return 0;
        }

        [PlugMethod(Assembler = typeof(CPUZeroFillAsm))]
        // TODO: implement this using REP STOSB and REPO STOSD
        public static void ZeroFill(uint aStartAddress, uint aLength) {
        }

        [PlugMethod(Assembler = typeof(CPUInitFloatAsm))]
        public static void InitFloat(Cosmos.CPU.Processor aThis) {
        }

        [PlugMethod(Assembler = typeof(CPUInitSSEAsm))]
        public static void InitSSE(Cosmos.CPU.Processor aThis) {
        }

        [PlugMethod(Assembler = typeof(CPUHaltAsm))]
        public static void Halt(Cosmos.CPU.Processor aThis) {
        }

        [PlugMethod(Assembler = typeof(CPUDisableINTsAsm))]
        public static void DoDisableInterrupts() {
        }

        [PlugMethod(Assembler = typeof(CPUEnableINTsAsm))]
        public static void DoEnableInterrupts() {
        }
    }
}
