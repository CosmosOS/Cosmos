using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Serializable]
	public class MemoryOperand: InstructionOperand
	{
		public MemoryOperand()
		{
			Scale = 1;
		}

		/// <summary>
		/// Whether address is formed by next instruction pointer + disp
		/// </summary>
		public bool RipBased { get; set; }

		public int Displacement { get; set; }
		public int DisplacementSize {
			get
			{
				if (Displacement == 0) return 0;
				if (((byte)Displacement) == Displacement) return 1;
				if (((short)Displacement) == Displacement) return 2;
				return 4;
			}
		}

		public int Scale { get; set; }
		public GeneralPurposeRegister Index { get; set; }
		public GeneralPurposeRegister Base { get; set; }

		public bool RequiresSIB()
		{
			if (RipBased) return false;
			if (Index == null)
			{
				if ((Displacement == 0) && ((Base.Register & Registers.OldRegsMask) == Registers.BP)) return true;
				if ((Base.Register & Registers.OldRegsMask) == Registers.SP) return true;
				return false;
			}
			return Base != null;
		}
	}
}
