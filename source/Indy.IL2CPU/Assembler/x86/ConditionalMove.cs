using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
	[OpCode(0xFFFFFFFF, "cmov")]
	public class ConditionalMove : Instruction
	{
		private string mDestination;
		private string mSource;
		private Condition mCondition;
		public ConditionalMove(Condition aCondition, string aDestination, string aSource)
		{
			mDestination = aDestination;
			mSource = aSource;
			mCondition = aCondition;
		}

		public override string ToString()
		{
			return string.Format("cmov{0} {1}, {2}", mCondition.GetMnemonics(), mDestination, mSource);
		}
	}
}
