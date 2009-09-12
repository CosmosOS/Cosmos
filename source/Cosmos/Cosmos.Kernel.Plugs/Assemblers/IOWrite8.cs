using System;
using System.Collections.Generic;
using System.Text;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

using CosAssembler = Cosmos.IL2CPU.Assembler;
using CosCPUAll = Cosmos.IL2CPU;
using CosCPUx86 = Cosmos.IL2CPU.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
  public sealed class IOWrite8: AssemblerMethod {
    public override void Assemble(Assembler aAssembler) {
      //TODO: This is a lot of work to write to a single port. We need to have some kind of inline ASM option that can emit a single out instruction
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0xC, SourceIsIndirect = true };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceDisplacement = 0x8, SourceIsIndirect = true };
      new CPUx86.Out { DestinationReg = CPUx86.Registers.AL };
    }

    public override void AssembleNew(object aAssembler) {
      //TODO: This is a lot of work to write to a single port. We need to have some kind of inline ASM option that can emit a single out instruction
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EDX, SourceReg = CosCPUx86.Registers.EBP, SourceDisplacement = 0xC, SourceIsIndirect = true };
      new CosCPUx86.Move { DestinationReg = CosCPUx86.Registers.EAX, SourceReg = CosCPUx86.Registers.EBP, SourceDisplacement = 0x8, SourceIsIndirect = true };
      new CosCPUx86.Out { DestinationReg = CosCPUx86.Registers.AL };
    }
  }
}