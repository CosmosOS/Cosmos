using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Assembler = Cosmos.IL2CPU.Assembler;
using CPUx86 = Cosmos.IL2CPU.X86;

namespace Indy.IL2CPU.X86.Plugs.CustomImplementations.System.Assemblers {
	public class Buffer_BlockCopy: AssemblerMethod {

		/*public static void BlockCopy(
		 *			Array src, [ebp + 24]
		 *			int srcOffset, [ebp + 20]
		 *			Array dst, [ebp + 16]
		 *			int dstOffset, [ebp + 12]
		 *			int count); [ebp + 8]
		 */
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
            new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 24 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 16 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 20 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EAX };

            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 16 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 16 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 12 };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX };

            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
		}

    public override void AssembleNew(object aAssembler) {
      new CPUx86.Move { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 24 };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceValue = 16 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 20 };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.ESI, SourceReg = CPUx86.Registers.EAX };

      new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 16 };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceValue = 16 };
      new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 12 };
      new CPUx86.Add { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EAX };

      new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
      new CPUx86.Movs { Size = 8, Prefixes = CPUx86.InstructionPrefixes.Repeat };
    }
	}
}