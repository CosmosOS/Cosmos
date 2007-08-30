using System.IO;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0x90)]
	public class Noop: Instruction {
		public override string ToString() {
			return "Noop";
		}

		// In fact this can be removed. EmitParams override is optional,its just here for example for the time being
		public override void EmitParams(BinaryWriter aWriter) {
			aWriter.Write("EmitParams in Assembler.X86.Noop");
		}
	}
}