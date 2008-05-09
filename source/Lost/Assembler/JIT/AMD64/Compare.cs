using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class Compare : AluInstruction
	{
		public Compare(InstructionOperand dest, InstructionOperand source)
			: base(dest, source)
		{
		}

		public override byte AccumulatorOpCode
		{
			get
			{
				return 0x3C;
			}
		}
		public override byte ImmediateExt
		{
			get { return 0x07; }
		}
		public override byte ImmediateOpCode
		{
			get { return 0x80; }
		}
		public override byte RegisterOpCode
		{
			get { return 0x38; }
		}

		public override string OpCodeFASM
		{
			get { return "cmp"; }
		}
	}
}
