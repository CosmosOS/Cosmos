using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// Represents the JGE opcode
	/// </summary>
	[OpCode(0xFFFFFFFF, "jge")]
	public class JumpIfGreaterOrEqual : JumpBase
	{
		public JumpIfGreaterOrEqual(string aAddress)
			: base(aAddress)
		{
		}
		public override string ToString()
		{
			return "jge " + Address;
		}
	}
}
