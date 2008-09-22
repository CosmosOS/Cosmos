using System.IO;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0x90, "nop")]
	public class Noop: Instruction {
	}
}