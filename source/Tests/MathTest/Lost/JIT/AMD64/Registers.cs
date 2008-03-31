using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lost.JIT.AMD64
{
	[Flags, Serializable]
	public enum Registers
	{
		None = 0,
		AX	= 1 << 0,
		BX	= 1 << 3,
		CX	= 1 << 1,
		DX	= 1 << 2,
		SI	= 1 << 6,
		DI	= 1 << 7,
		SP	= 1 << 4,
		BP	= 1 << 5,
		R8	= 1 << 8,
		R9	= 1 << 9,
		R10	= 1 << 10,
		R11	= 1 << 11,
		R12	= 1 << 12,
		R13 = 1 << 13,
		R14	= 1 << 14,
		R15	= 1 << 15,
		OldRegsMask = AX | BX | CX | DX | SI | DI | SP | BP,
		NewRegsMask = R8 | R9 | R10 | R11 | R12 | R13 | R14 | R15,
	}

	public static class RegistersExtensions
	{
		static readonly Dictionary<int, int> regIndex = new Dictionary<int, int>();
		static RegistersExtensions()
		{
			for (int i = 0; i < 16; i++)
				regIndex.Add(1 << i, i);
		}

		public static int GetIndex(this Registers register)
		{
			register &= Registers.OldRegsMask;
			return regIndex[(int)register];
		}
	}
}
