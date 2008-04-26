using System;
using System.Collections.Generic;
using Indy.IL2CPU.Plugs;
using Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class DoTest: AssemblerMethod {
		public override void Assemble(Indy.IL2CPU.Assembler.Assembler aAssembler) {
			new CPUx86.Call("_CODE_REQUESTED_BREAK_");
			new CPUx86.Move("eax", "0x10");
			new CPUx86.Move("ds", "ax");
			new CPUx86.Move("eax", "0x18");
			new CPUx86.Move("es", "ax");
			new CPUx86.Move("eax", "0x20");
			new CPUx86.Move("fs", "ax");
			new CPUx86.Move("eax", "0x28");
			new CPUx86.Move("gs", "ax");
			new CPUx86.Move("eax", "0x30");
			new CPUx86.Move("ss", "ax");
			new CPUx86.Move("eax", "1");
			new CPUx86.Move("ebx", "2");
			new CPUx86.Move("ecx", "3");
			new CPUx86.Move("edx", "4");
			new CPUx86.Move("esi", "5");
			new CPUx86.Move("edi", "6");
			new CPUx86.Move("ebp", "7");
			new CPUx86.Noop();
			new Label("DO_THE_TEST");
			new CPUx86.Interrupt(0x35);
		}
	}
}
