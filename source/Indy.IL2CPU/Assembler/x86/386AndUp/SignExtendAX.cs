using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "cdq")]
	public class SignExtendAX : New_Instruction
	{
		private int mOldSize;
		public SignExtendAX(int aOldSize)
		{
			mOldSize = aOldSize;
		}
		public override string ToString()
		{
			switch (mOldSize)
			{
			case 4:
				return "cdq";
			case 2:
				return "cwd";
			default:
				throw new NotSupportedException();
			}
		}
	}
}