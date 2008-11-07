using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "mov")]
	public class Move: InstructionWithDestinationAndSource {
        public override string ToString() {
			if (Size==0) {
				return "mov " + GetDestinationAsString() + ", " + GetSourceAsString();
			} else {
                return "mov " + SizeToString(Size) + " " + GetDestinationAsString() + ", " + GetSourceAsString();
			}
		}
	}
}
