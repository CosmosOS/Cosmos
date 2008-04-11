using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class And : AluInstruction
	{
		public And(InstructionOperand dest, InstructionOperand source)
			: base(dest, source)
		{
		}
		public And(InstructionOperand dest, byte source)
			: base(dest, source)
		{
		}
		public And(InstructionOperand dest, short source)
			: base(dest, source)
		{
		}
		public And(InstructionOperand dest, int source)
			: base(dest, source)
		{
		}
		public And(InstructionOperand dest, long source)
			: base(dest, source)
		{
		}

		public override byte AccumulatorOpCode
		{
			get
			{
				return 0x24;
			}
		}
		public override byte ImmediateExt
		{
			get { return 0x04; }
		}
		public override byte ImmediateOpCode
		{
			get { return 0x80; }
		}
		public override byte RegisterOpCode
		{
			get { return 0x20; }
		}

		public override string OpCodeFASM
		{
			get { return "and"; }
		}
	}
}