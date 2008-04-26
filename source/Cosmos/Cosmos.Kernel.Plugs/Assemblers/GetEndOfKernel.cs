using System;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetEndOfKernel: AssemblerMethod {
		public override void Assemble(Assembler aAssembler) {
			new CPUx86.Pushd("_end_data");
		}
	}
}