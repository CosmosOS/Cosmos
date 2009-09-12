using System;

using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Assembler = Indy.IL2CPU.Assembler.Assembler;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMEnablePSE: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.CR4 };
      new CPUx86.Or { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0x00000010 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.CR4, SourceReg = CPUx86.Registers.EAX };
    }

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceReg = CosCPUx86.Registers.CR4 };
      new CosCPUx86.Or { DestinationReg = CosCPUx86.Registers.EAX, SourceValue = 0x00000010 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.CR4, SourceReg = CosCPUx86.Registers.EAX };
    }
  }
}