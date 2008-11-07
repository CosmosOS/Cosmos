using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "push")]
    public class Push : InstructionWithDestinationAndSize {
		public override string ToString() {
            if (Size > 0) {
                return "push " + SizeToString(Size) + " " + GetDestinationAsString();
            }
            return "push dword " + GetDestinationAsString();
		} 
	}
}