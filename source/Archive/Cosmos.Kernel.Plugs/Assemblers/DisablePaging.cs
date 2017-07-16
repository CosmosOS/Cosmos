using System;

using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Assembler.x86;
using Assembler = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ASMDisablePaging: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Mov(XSRegisters.EAX, XSRegisters.CPUx86.Registers.CR0);
      XS.And(XSRegisters.EAX, 0x7FFFFFFF);
      XS.Mov(XSRegisters.CR0, XSRegisters.CPUx86.Registers.EAX);
    }
  }
}