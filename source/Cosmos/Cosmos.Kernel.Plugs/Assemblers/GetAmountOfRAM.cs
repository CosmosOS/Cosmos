using System;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetAmountOfRAM: AssemblerMethod {
		public override void Assemble(Assembler aAssembler) {
			new CPUx86.Move("eax", "[MultiBootInfo_Memory_High]");
			new CPUx86.Xor(CPUx86.Registers.EDX, CPUx86.Registers.EDX);
			new CPUx86.Move("ecx", "1024");
			new CPUx86.Divide("ecx");
			new CPUx86.Add("eax", "1");
			new CPUx86.Pushd(CPUx86.Registers.EAX);
		}
	}
}
