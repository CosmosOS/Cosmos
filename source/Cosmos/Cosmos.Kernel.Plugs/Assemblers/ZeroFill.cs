using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Compiler.Assembler.Assembler;
using CPUAll = Cosmos.Compiler.Assembler;
using CPUx86 = Cosmos.Compiler.Assembler.X86;

using CosAssembler = Cosmos.Compiler.Assembler.Assembler;
using CosCPUAll = Cosmos.Compiler.Assembler;
using CosCPUx86 = Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ZeroFill: AssemblerMethod {

    //		public static void ZeroFill(uint aStartAddress, uint aLength) {}
      public override void AssembleNew(object aAssembler, object aMethodInfo)
      {
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
}