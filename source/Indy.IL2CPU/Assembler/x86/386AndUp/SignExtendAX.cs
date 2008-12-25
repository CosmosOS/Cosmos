using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode("cdq")]
	public class SignExtendAX : InstructionWithSize
	{
		public override string ToString()
		{
			switch (Size)
			{
			case 32:
				return "cdq";
			case 16:
				return "cwde";
                case 8:
                return "cbw";
			default:
				throw new NotSupportedException();
			}
		}
	}
}