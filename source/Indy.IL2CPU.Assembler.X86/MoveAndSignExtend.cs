using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "movsx")]
	public class MoveAndSignExtend : Instruction
	{
		private string mDest;
		private string mSource;
		public MoveAndSignExtend(string aDest, string aSource)
		{
			mDest = aDest;
			mSource = aSource;
		}
		public override string ToString()
		{
			return string.Format("movsx {0}, {1}", mDest, mSource);
		}
	}
}