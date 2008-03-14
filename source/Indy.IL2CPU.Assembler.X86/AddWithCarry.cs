using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "adc")]
	public class Add : Instruction
	{
		public readonly string Address1;
		public readonly string Address2;
		public Add(string aAddress1, string aAddress2)
		{
			Address1 = aAddress1;
			Address2 = aAddress2;
		}
		public override string ToString()
		{
			return "adc " + Address1 + "," + Address2;
		}
	}
}
