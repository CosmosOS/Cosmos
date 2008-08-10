using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "movzx")]
	public class MoveAndZeroExtend : Instruction
	{
		private string mDest;
		private string mSource;
		public MoveAndZeroExtend(string aDest, string aSource)
		{
			mDest = aDest;
			mSource = aSource;
		}
		public override string ToString()
		{
			return string.Format("movzx {0}, {1}", mDest, mSource);
		}
	}
}
