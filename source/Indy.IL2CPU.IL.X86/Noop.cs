using System.IO;
using Mono.Cecil.Cil;
using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	public class Noop: IL.Noop {
		public override void Assemble(Instruction aInstruction, BinaryWriter aOutput) {
			new Assembler.X86.Noop().EmitParams(aOutput);
		}
	}
}