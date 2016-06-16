using System;
using Cosmos.IL2CPU.Plugs;
using Assembler = Cosmos.Assembler;
using CPUx86 = Cosmos.Assembler.x86;
using CPUAll = Cosmos.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class GetAmountOfRAM: AssemblerMethod {
    public override void AssembleNew(Cosmos.Assembler.Assembler aAssembler, object aMethodInfo) {
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("MultiBootInfo_Memory_High"), SourceIsIndirect = true };
      new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
      XS.Mov(XSRegisters.ECX, 1024);
      XS.Divide(XSRegisters.ECX);
      XS.Add(XSRegisters.EAX, 1);
      XS.Push(XSRegisters.EAX);
    }

  }
}