using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	/// <summary>
	/// Represents the JLE opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jle")]
	public class JumpIfLessOrEqual: JumpBase {
	}
}