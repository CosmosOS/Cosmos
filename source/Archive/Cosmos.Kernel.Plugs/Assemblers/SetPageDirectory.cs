using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Assembler = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMSetPageDirectory: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Mov(XSRegisters.EAX, XSRegisters.ESP, sourceDisplacement: 0x8);
      XS.Mov(XSRegisters.CR3, XSRegisters.CPUx86.Registers.EAX);
    }
  }
}