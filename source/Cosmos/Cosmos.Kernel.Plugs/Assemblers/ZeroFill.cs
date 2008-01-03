using System;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class ZeroFill: AssemblerMethod {

		//		public static void ZeroFill(uint aStartAddress, uint aLength) {}
		public override void Assemble(Assembler aAssembler) {
			new CPUx86.Move("edi", "[ebp + 0xC]"); // address
			new CPUx86.Move("ecx", "[ebp + 8]");  // length
			new CPUx86.Move("eax", "0");
			new CPUx86.RepeatStosb();						
		}
	}
}
