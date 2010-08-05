using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Assembler = Cosmos.Compiler.Assembler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMInvalidatePage: AssemblerMethod {
  
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 0xC };
        //TODO: new InvalPG { DestinationReg = Registers.EAX };
    }
  }
}