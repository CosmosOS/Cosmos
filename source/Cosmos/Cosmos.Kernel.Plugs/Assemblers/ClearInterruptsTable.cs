using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using CPUAll = Cosmos.Compiler.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public class ClearInterruptsTable: AssemblerMethod {

    public override void AssembleNew(object aAssembler, object aMethodInfo) {
      new CPUx86.ClrInterruptFlag();
      new CPUx86.Move { DestinationRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer"), DestinationIsIndirect = true, Size = 16, SourceValue = 0 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = CPUAll.ElementReference.New("_NATIVE_IDT_Pointer") };
      new CPUx86.Lidt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
      new CPUx86.Sti();
    }
  }
}
