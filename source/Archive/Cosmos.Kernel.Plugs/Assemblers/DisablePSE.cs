using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Assembler = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMDisablePSE: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Mov(XSRegisters.EAX, XSRegisters.CPUx86.Registers.CR4);
      XS.And(XSRegisters.EAX, 0xFFFFFFEF);
      XS.Mov(XSRegisters.CR4, XSRegisters.CPUx86.Registers.EAX);
 
    }
  }
}