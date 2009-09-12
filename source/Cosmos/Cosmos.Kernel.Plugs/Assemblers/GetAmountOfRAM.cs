using System;
using Indy.IL2CPU.Plugs;

using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUAll = Indy.IL2CPU.Assembler;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class GetAmountOfRAM: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("MultiBootInfo_Memory_High"), SourceIsIndirect = true };
      new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 1024 };
      new CPUx86.Divide { DestinationReg = CPUx86.Registers.ECX };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
      new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
    }

    public override void AssembleNew(object aAssembler) {
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceRef = CosCPUAll.ElementReference.New("MultiBootInfo_Memory_High"), SourceIsIndirect = true };
      new CosCPUx86.Xor { DestinationReg = CosCPUx86.Registers.EDX, SourceReg = CosCPUx86.Registers.EDX };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.ECX, SourceValue = 1024 };
      new CosCPUx86.Divide { DestinationReg = CosCPUx86.Registers.ECX };
      new CosCPUx86.Add { DestinationReg = CosCPUx86.Registers.EAX, SourceValue = 1 };
      new CosCPUx86.Push { DestinationReg = CosCPUx86.Registers.EAX };
    } 
  }
}