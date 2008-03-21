using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Lost.JIT.AMD64
{
	static class ModRM
	{
		public static void EncodeRegisters(this Stream dest, Register destReg, Register sourceReg)
		{
			destReg &= Register.Legacy;
			sourceReg &= Register.Legacy;

			byte result = 0xC0;			//11000000b
			result |= (byte)(((byte)destReg) << 3 | (byte)sourceReg);
			dest.WriteByte(result);
		}
		public static void EncodeIndirectMemory(this Stream dest, byte xcode, Register baseReg)
		{
			Debug.Assert((xcode & ~(byte)Register.Legacy) == 0);

			byte modRM =(byte)((xcode << 3) | (byte)baseReg);
			dest.WriteByte(modRM);
		}
	}
}
