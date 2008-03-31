using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class AddWithCarry: DestSourceInstruction
	{
		public AddWithCarry(InstructionOperand dest, InstructionOperand source): base(dest, source)
		{
		}
		public AddWithCarry(InstructionOperand dest, byte source)
			: base(dest, source)
		{
		}
		public AddWithCarry(InstructionOperand dest, short source)
			: base(dest, source)
		{
		}
		public AddWithCarry(InstructionOperand dest, int source)
			: base(dest, source)
		{
		}
		public AddWithCarry(InstructionOperand dest, long source)
			: base(dest, source)
		{
		}

		public override byte AccumulatorOpCode
		{
			get
			{
				return 0x14;
			}
		}
		public override byte ImmediateExt
		{
			get { return 0x02; }
		}
		public override byte ImmediateOpCode
		{
			get { return 0x80; }
		}
		public override byte RegisterOpCode
		{
			get { return 0x10; }
		}
	}
}
