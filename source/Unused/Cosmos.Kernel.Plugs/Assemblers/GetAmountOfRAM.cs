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
      new CPUx86.Mov { DestinationReg = CPUx86.Registers.ECX, SourceValue = 1024 };
      new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
    }

  }
}