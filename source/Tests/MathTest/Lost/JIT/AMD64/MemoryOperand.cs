using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class MemoryOperand: InstructionOperand
	{
		/// <summary>
		/// Whether address is formed by next instruction pointer + disp
		/// </summary>
		public bool RipBased { get; set; }

		public int Displacement { get; set; }
		public int DisplacementSize { get; set; }

		public int Scale { get; set; }
		public GeneralPurposeRegister Index { get; set; }
		public GeneralPurposeRegister Base { get; set; }

		public bool RequiresSIB()
		{
			return (Index != null || Base != null);
		}
	}
}
