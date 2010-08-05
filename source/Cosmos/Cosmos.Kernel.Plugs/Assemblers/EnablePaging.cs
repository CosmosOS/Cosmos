using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Assembler = Cosmos.Compiler.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMEnablePaging: AssemblerMethod {
    
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.CR0 };
      new CPUx86.Or { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0x80000000 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.CR0, SourceReg = CPUx86.Registers.EAX };
    } 

  }
}