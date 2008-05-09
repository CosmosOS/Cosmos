using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class Add : AluInstruction
	{
		public Add(InstructionOperand dest, InstructionOperand source)
			: base(dest, source)
		{
		}

		public override byte AccumulatorOpCode
		{
			get
			{
				return 0x04;
			}
		}
		public override byte ImmediateExt
		{
			get { return 0x00; }
		}
		public override byte ImmediateOpCode
		{
			get { return 0x80; }
		}
		public override byte RegisterOpCode
		{
			get { return 0x00; }
		}

		public override string OpCodeFASM
		{
			get { return "adc"; }
		}
	}
}
