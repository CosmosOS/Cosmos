using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public sealed class IORead8: AssemblerMethod {
    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      //TODO: This is a lot of work to read a port. We need to have some kind of inline ASM option that can emit a single out instruction
      //TODO: Also make an attribute that forces normal inlining fo a method
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 0x8 };
      //TODO: Do we need to clear rest of EAX first?
      //    MTW: technically not, as in other places, it _should_ be working with AL too..
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
      new CPUx86.In { DestinationReg = CPUx86.Registers.AL };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
    }  
  }
}