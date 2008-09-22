using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "jc")]
	public class JumpIfCarry : JumpBase
	{
		public JumpIfCarry(string aTarget)
			: base(aTarget)
		{
		}
		public override string ToString()
		{
			return "jc " + Address;
		}
	}
}
