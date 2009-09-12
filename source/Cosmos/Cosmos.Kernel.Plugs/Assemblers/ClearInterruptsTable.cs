using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUAll = Indy.IL2CPU.Assembler;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ClearInterruptsTable: AssemblerMethod {

    public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
      new CPUx86.ClrInterruptFlag();
      new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer"), DestinationIsIndirect = true, Size = 16, SourceValue = 0 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer") };
      new CPUx86.Lidt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
      new CPUx86.Sti();
    }

    public override void AssembleNew(object aAssembler) {
      new CPUx86.ClrInterruptFlag();
      new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer"), DestinationIsIndirect = true, Size = 16, SourceValue = 0 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer") };
      new CPUx86.Lidt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
      new CPUx86.Sti();
    }
  }
}
