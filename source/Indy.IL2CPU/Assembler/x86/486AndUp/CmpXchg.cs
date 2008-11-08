using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "cmpxchg")]
	public class CmpXchg: InstructionWithDestinationAndSourceAndSize {
	}
}