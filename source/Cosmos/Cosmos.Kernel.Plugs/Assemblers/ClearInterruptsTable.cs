using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Cosmos.Kernel.Plugs.Assemblers {
    public class ClearInterruptsTable : AssemblerMethod {

        public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new CPUx86.ClrInterruptFlag();
            new CPUx86.Move { DestinationRef = new ElementReference("_NATIVE_IDT_Pointer"), DestinationIsIndirect = true, Size = 16, SourceValue = 0 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceRef = new ElementReference("_NATIVE_IDT_Pointer") };
            new CPUx86.Lidt { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
            new CPUx86.Sti();
        }
    }
}
