using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class ImmediateOperand: InstructionOperand
	{
		public ImmediateOperand(long value)
		{
			this.Value = value;
			this.Size = 8;
		}
		public ImmediateOperand(int value)
		{
			this.Value = value;
			Size = 4;
		}
		public ImmediateOperand(short value)
		{
			this.Value = value;
			Size = 2;
		}
		public ImmediateOperand(byte value)
		{
			this.Value = value;
			Size = 1;
		}

		public long Value { get; private set; }
		public int Size { get; private set; }

		public override string ToString()
		{
			string digs = (Size * 2).ToString();
			return "0x" + Value.ToString("X" + digs);
		}
	}
}
