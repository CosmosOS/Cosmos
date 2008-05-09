using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class InstructionOperand
	{
		public static implicit operator InstructionOperand(byte value)
		{
			return new ImmediateOperand(value);
		}
		public static implicit operator InstructionOperand(short value)
		{
			return new ImmediateOperand(value);
		}
		public static implicit operator InstructionOperand(int value)
		{
			return new ImmediateOperand(value);
		}
		public static implicit operator InstructionOperand(long value)
		{
			return new ImmediateOperand(value);
		}
	}
}
