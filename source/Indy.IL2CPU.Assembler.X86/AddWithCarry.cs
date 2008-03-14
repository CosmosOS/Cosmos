using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "adc")]
	public class AddWithCarry : Instruction
	{
		public readonly string Dest;
		public readonly string Source;
		public AddWithCarry(string dest, string source)
		{
			Dest = dest;
			Source = source;
		}
		public override string ToString()
		{
			return "adc " + Dest + "," + Source;
		}
	}
}
