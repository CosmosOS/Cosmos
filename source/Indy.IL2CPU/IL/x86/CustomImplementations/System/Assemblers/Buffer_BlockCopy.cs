using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Assemblers {
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
            new CPUx86.Add { DestinationReg = Registers.ESI, SourceValue = 16 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 20 };
			new CPUx86.Add{DestinationReg = Registers.ESI, SourceReg=Registers.EAX};

            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDI, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 16 };
            new CPUx86.Add { DestinationReg = Registers.EDI, SourceValue = 16 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 12 };
            new CPUx86.Add { DestinationReg = Registers.EDI, SourceReg = Registers.EAX };

            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceReg = CPUx86.Registers.EBP, SourceIsIndirect = true, SourceDisplacement = 8 };
            new Movs { Size = 8, Prefixes = InstructionPrefixes.Repeat };
		}
	}
}