using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
	/// <summary>
	/// Represents the JMP opcode
	/// </summary>
    [Cosmos.Assembler.OpCode("Jmp")]
	public class Jump: JumpBase {
	}
}
