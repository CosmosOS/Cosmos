using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public sealed class IOWrite8: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      //TODO: This is a lot of work to write to a single port. We need to have some kind of inline ASM option that can emit a single out instruction
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0xC, SourceIsIndirect = true };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x8, SourceIsIndirect = true };
      new CPUx86.Out { DestinationReg = CPUx86.Registers.AL };
    }
  }
}