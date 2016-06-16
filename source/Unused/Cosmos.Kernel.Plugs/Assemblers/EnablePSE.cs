using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Assembler = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMEnablePSE: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.CR4 };
      XS.Or(XSRegisters.EAX, 0x00000010);
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.CR4, SourceReg = CPUx86.Registers.EAX };
    }
  }
}