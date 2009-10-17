using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.IL2CPU.X86;
using Assembler = Cosmos.IL2CPU.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMDisablePaging: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.CR0 };
      new CPUx86.And { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0x7FFFFFFF };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.CR0, SourceReg = CPUx86.Registers.EAX };
    }
  }
}