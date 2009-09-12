using System;

using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Assembler = Indy.IL2CPU.Assembler.Assembler;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMSetPageDirectory: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 0x8 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.CR3, SourceReg = CPUx86.Registers.EAX };
    }

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceReg = CosCPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 0x8 };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.CR3, SourceReg = CosCPUx86.Registers.EAX };
    }
  }
}