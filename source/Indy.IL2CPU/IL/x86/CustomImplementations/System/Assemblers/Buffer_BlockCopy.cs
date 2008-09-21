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
			new CPUx86.Move("esi", "[ebp + 24]"); // src
			new CPUx86.Add("esi", "16");
			new CPUx86.Move("eax", "[ebp + 20]");
			new CPUx86.Add("esi", "eax");

			new CPUx86.Move("edi", "[ebp + 16]");
			new CPUx86.Add("edi", "16");
			new CPUx86.Move("eax", "[ebp + 12]");
			new CPUx86.Add("edi", "eax");

			new CPUx86.Move("ecx", "[ebp + 8]");
			new CPUx86.RepeatMovsb();
		}
	}
}