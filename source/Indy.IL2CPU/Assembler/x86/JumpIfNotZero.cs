using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// Represents the JNZ opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jnz")]
	public class JumpIfNotZero : JumpBase
	{
	}
}
