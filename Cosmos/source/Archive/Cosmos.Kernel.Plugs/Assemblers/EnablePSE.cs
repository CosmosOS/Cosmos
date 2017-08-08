using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Assembler = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMEnablePSE: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Mov(XSRegisters.EAX, XSRegisters.CPUx86.Registers.CR4);
      XS.Or(XSRegisters.EAX, 0x00000010);
      XS.Mov(XSRegisters.CR4, XSRegisters.CPUx86.Registers.EAX);
    }
  }
}