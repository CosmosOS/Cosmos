using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using CPUAll = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class GetAmountOfRAM: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      XS.Mov(XSRegisters.EAX, CPUAll.ElementReference.New("MultiBootInfo_Memory_High"), sourceIsIndirect: true);
      XS.Xor(XSRegisters.EDX, XSRegisters.CPUx86.Registers.EDX);
      XS.Mov(XSRegisters.ECX, 1024);
      XS.Divide(XSRegisters.ECX);
      XS.Add(XSRegisters.EAX, 1);
      XS.Push(XSRegisters.EAX);
    }

  }
}