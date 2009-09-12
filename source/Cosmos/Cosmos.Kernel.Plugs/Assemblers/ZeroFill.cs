using System;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUAll = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ZeroFill: AssemblerMethod {

    //		public static void ZeroFill(uint aStartAddress, uint aLength) {}
    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
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

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.ClrDirFlag();
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDI, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0xC }; //address
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.ECX, SourceReg = CosCPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x8 }; //length
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceValue = 0 };
      new CosCPUx86.ShiftRight { DestinationReg = CosCPUx86.Registers.ECX, SourceValue = 1 };
      new CosCPUx86.ConditionalJump { Condition = CosCPUx86.ConditionalTestEnum.Below, DestinationLabel = ".step2" };
      new CosCPUx86.StoreByteInString();
      new CosCPUAll.Label(".step2");
      new CosCPUx86.ShiftRight { DestinationReg = CosCPUx86.Registers.ECX, SourceValue = 1 };
      new CosCPUx86.ConditionalJump { Condition = CosCPUx86.ConditionalTestEnum.Below, DestinationLabel = ".step3" };
      new CosCPUx86.StoreWordInString();
      new CosCPUAll.Label(".step3");
      new CosCPUx86.Stos { Size = 32, Prefixes = CosCPUx86.InstructionPrefixes.Repeat };
    }
  }
}