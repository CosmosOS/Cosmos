using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public sealed class GeneralPurposeRegister: InstructionOperand
	{
		public GeneralPurposeRegister(Registers register, int size)
		{
			this.Register = register;
			this.Size = size;
		}

		public int Size { get; private set; }
		public Registers Register { get; private set; }
	}
}
