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

		public static readonly GeneralPurposeRegister RAX = new GeneralPurposeRegister(Registers.AX, 8);
		public static readonly GeneralPurposeRegister RBX = new GeneralPurposeRegister(Registers.BX, 8);
		public static readonly GeneralPurposeRegister RCX = new GeneralPurposeRegister(Registers.CX, 8);
		public static readonly GeneralPurposeRegister RDX = new GeneralPurposeRegister(Registers.DX, 8);
		public static readonly GeneralPurposeRegister RSI = new GeneralPurposeRegister(Registers.SI, 8);
		public static readonly GeneralPurposeRegister RDI = new GeneralPurposeRegister(Registers.DI, 8);
		public static readonly GeneralPurposeRegister RBP = new GeneralPurposeRegister(Registers.BP, 8);
		public static readonly GeneralPurposeRegister RSP = new GeneralPurposeRegister(Registers.SP, 8);
		public static readonly GeneralPurposeRegister R8 = new GeneralPurposeRegister(Registers.R8, 8);
		public static readonly GeneralPurposeRegister R9 = new GeneralPurposeRegister(Registers.R9, 8);
		public static readonly GeneralPurposeRegister R10 = new GeneralPurposeRegister(Registers.R10, 8);
		public static readonly GeneralPurposeRegister R11 = new GeneralPurposeRegister(Registers.R11, 8);
		public static readonly GeneralPurposeRegister R12 = new GeneralPurposeRegister(Registers.R12, 8);
		public static readonly GeneralPurposeRegister R13 = new GeneralPurposeRegister(Registers.R13, 8);
		public static readonly GeneralPurposeRegister R14 = new GeneralPurposeRegister(Registers.R14, 8);
		public static readonly GeneralPurposeRegister R15 = new GeneralPurposeRegister(Registers.R15, 8);

		public static readonly GeneralPurposeRegister EAX = new GeneralPurposeRegister(Registers.AX, 4);
		public static readonly GeneralPurposeRegister EBX = new GeneralPurposeRegister(Registers.BX, 4);
		public static readonly GeneralPurposeRegister ECX = new GeneralPurposeRegister(Registers.CX, 4);
		public static readonly GeneralPurposeRegister EDX = new GeneralPurposeRegister(Registers.DX, 4);
		public static readonly GeneralPurposeRegister ESI = new GeneralPurposeRegister(Registers.SI, 4);
		public static readonly GeneralPurposeRegister EDI = new GeneralPurposeRegister(Registers.DI, 4);
		public static readonly GeneralPurposeRegister EBP = new GeneralPurposeRegister(Registers.BP, 4);
		public static readonly GeneralPurposeRegister ESP = new GeneralPurposeRegister(Registers.SP, 4);

		public static readonly GeneralPurposeRegister AX = new GeneralPurposeRegister(Registers.AX, 2);
		public static readonly GeneralPurposeRegister BX = new GeneralPurposeRegister(Registers.BX, 2);
		public static readonly GeneralPurposeRegister CX = new GeneralPurposeRegister(Registers.CX, 2);
		public static readonly GeneralPurposeRegister DX = new GeneralPurposeRegister(Registers.DX, 2);
		public static readonly GeneralPurposeRegister SI = new GeneralPurposeRegister(Registers.SI, 2);
		public static readonly GeneralPurposeRegister DI = new GeneralPurposeRegister(Registers.DI, 2);
		public static readonly GeneralPurposeRegister BP = new GeneralPurposeRegister(Registers.BP, 2);
		public static readonly GeneralPurposeRegister SP = new GeneralPurposeRegister(Registers.SP, 2);

		public static readonly GeneralPurposeRegister AL = new GeneralPurposeRegister(Registers.AX, 1);
		public static readonly GeneralPurposeRegister BL = new GeneralPurposeRegister(Registers.BX, 1);
		public static readonly GeneralPurposeRegister CL = new GeneralPurposeRegister(Registers.CX, 1);
		public static readonly GeneralPurposeRegister DL = new GeneralPurposeRegister(Registers.DX, 1);
		//public static readonly GeneralPurposeRegister SI = new GeneralPurposeRegister(Registers.SI, 1);
		//public static readonly GeneralPurposeRegister DI = new GeneralPurposeRegister(Registers.DI, 1);
		//public static readonly GeneralPurposeRegister BP = new GeneralPurposeRegister(Registers.BP, 1);
		//public static readonly GeneralPurposeRegister SP = new GeneralPurposeRegister(Registers.SP, 1);
	}
}
