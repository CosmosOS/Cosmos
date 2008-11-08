using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JE opcode
	/// </summary>
    [OpCode("je")]
	public class JumpIfEqual: JumpBase {
	}
}
