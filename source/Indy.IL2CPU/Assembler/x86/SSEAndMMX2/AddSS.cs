using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.SSE {
	[OpCode(0xFFFFFFFF, "addss")]
	public class AddSS: InstructionWithDestinationAndSource {
	}
}
