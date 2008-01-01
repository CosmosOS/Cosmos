using System;
using System.Linq;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System.Assemblers {
	public class Array_InternalCopy: AssemblerMethod {

		/* void Copy(Array sourceArray,			ebp + 0x1C
		 *			 int sourceIndex,			ebp + 0x18
		 *			 Array destinationArray,	ebp + 0x14
		 *			 int destinationIndex,		ebp + 0x10
		 *			 int length,				ebp + 0xC
		 *			 bool reliable);			ebp + 0x8
		 */
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			new CPUx86.Pushd("[ebp + 0x1C]");
			new CPUx86.Add("dword [esp]", "12"); // pointer is at the element size
			new CPUx86.Pop("eax");
			new CPUx86.Move("eax", "[eax]"); // element size
			new CPUx86.Move("ebx", "[ebp + 0x18]");
			new CPUx86.Multiply("ebx");
			new CPUx86.Add("eax", "16");
			new CPUx86.Move("esi", "[ebp + 0x1C]");
			new CPUx86.Add("esi", "eax"); // source ptr

			new CPUx86.Pushd("[ebp + 0x14]");
			new CPUx86.Add("dword [esp]", "12"); // pointer is at element size
			new CPUx86.Pop("eax");
			new CPUx86.Move("eax", "[eax]"); // element size
			new CPUx86.Move("ecx", "[ebp + 0x10]");
			new CPUx86.Multiply("ecx");
			new CPUx86.Add("eax", "16");
			new CPUx86.Move("edi", "[ebp + 0x14]");
			new CPUx86.Add("edi", "eax");

			// calculate byte count to copy
			new CPUx86.Move("eax", "[ebp + 0x14]");
			new CPUx86.Add("eax", "12");
			new CPUx86.Move("eax", "[eax]");
			new CPUx86.Move("edx", "[ebp + 0xC]");
			new CPUx86.Multiply("edx");
			new CPUx86.Move("ecx", "eax");
			new RepeatMovsd();
		}
	}
}
