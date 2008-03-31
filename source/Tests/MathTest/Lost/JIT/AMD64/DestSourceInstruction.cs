using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public abstract class DestSourceInstruction: ProcessorInstruction
	{
		public DestSourceInstruction(InstructionOperand dest, InstructionOperand source)
		{
			this.Dest = dest;
			this.Source = source;
		}

		public InstructionOperand Dest { get; private set; }
		public InstructionOperand Source { get; private set; }
	}
}
