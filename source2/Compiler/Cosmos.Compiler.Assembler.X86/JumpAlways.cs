using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
	/// <summary>
	/// Represents the JMP opcode
	/// </summary>
    [Cosmos.Assembler.OpCode("jmp")]
	public class Jump: JumpBase {
	}
}