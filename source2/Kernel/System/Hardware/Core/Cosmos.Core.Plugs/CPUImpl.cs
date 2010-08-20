using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Core.Plugs {
    [Plug(Target = typeof(Core.CPU))]
    public class CPUImpl {
        [PlugMethod(Assembler = typeof(Assemblers.CreateIDT))]
        public void CreateIDT(bool aEnableInterruptsImmediately) {
        }

        [PlugMethod(Assembler = typeof(Assemblers.CreateGDT))]
        public void CreateGDT() { }

        public class GetAmountOfRAMAsm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("MultiBootInfo_Memory_High"), SourceIsIndirect = true };
                new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 1024 };
                new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
                new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            }
        }
        [PlugMethod(Assembler = typeof(GetAmountOfRAMAsm))]
        public uint GetAmountOfRAM() { return 0; }

        public class GetEndOfKernelAsm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Push { DestinationRef = CPUAll.ElementReference.New("_end_code") };
            }
        }
        [PlugMethod(Assembler = typeof(GetEndOfKernelAsm))]
        public uint GetEndOfKernel() { return 0; }

        public class ZeroFillAsm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.ClrDirFlag();
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; //address
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x8 }; //length
                new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.ECX, SourceValue = 1 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = ".step2" };
                new CPUx86.StoreByteInString();
                new CPUAll.Label(".step2");
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.ECX, SourceValue = 1 };
                new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.Below, DestinationLabel = ".step3" };
                new CPUx86.StoreWordInString();
                new CPUAll.Label(".step3");
                new CPUx86.Stos { Size = 32, Prefixes = CPUx86.InstructionPrefixes.Repeat };
            }
        }
        [PlugMethod(Assembler = typeof(ZeroFillAsm))]
        // TODO: implement this using REP STOSB and REPO STOSD
        public void ZeroFill(uint aStartAddress, uint aLength) { }

        public class InitFloatAsm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.x87.FloatInit();
            }
        }
        [PlugMethod(Assembler = typeof(InitFloatAsm))]
        public void InitFloat() { }

        public class HaltAsm : AssemblerMethod {
            public override void AssembleNew(object aAssembler, object aMethodInfo) {
                new CPUx86.Halt();
            }
        }
        [PlugMethod(Assembler = typeof(HaltAsm))]
        public void Halt() { }


    //    [PlugMethod(Assembler = typeof(P2.Assemblers.GetEndOfStack))]
    //    public static uint GetEndOfStack() {
    //        return 0;
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCurrentESP))]
    //    public static uint GetCurrentESP() {
    //        return 0;
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.DoTest))]
    //    public static void DoTest() {
    //    }


    //    [PlugMethod(Assembler = typeof(P2.Assemblers.CPUIDSupport))]
    //    public static uint HasCPUIDSupport() {
    //        return 0;
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.GetCPUIDInternal))]
    //    public static void GetCPUId(out uint d, out uint c, out uint b, out uint a, uint v) {
    //        d = 0;
    //        c = 0;
    //        b = 0;
    //        a = 0;
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.Halt))]
    //    public static void Halt() { }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.DisableInterrupts))]
    //    public static void DisableInterrupts() {
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.EnableInterrupts))]
    //    public static void EnableInterrupts() {
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.Interrupt30))]
    //    public static void Interrupt30(ref uint aEAX, ref uint aEBX, ref uint aECX, ref uint aEDX) {
    //        aEAX = 0;
    //    }

    //    [PlugMethod(Assembler = typeof(P2.Assemblers.GetMBIAddress))]
    //    public static uint GetMBIAddress() {
    //        return 0;
    //    }
    }
}
