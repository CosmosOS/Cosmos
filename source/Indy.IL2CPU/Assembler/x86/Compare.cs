using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "cmp")]
	public class Compare: InstructionWithDestinationAndSource {
        public override string ToString() {
            if (Size == 0) {
                return "cmp " + GetDestinationAsString() + ", " + GetSourceAsString();
            } else {
                return "cmp " + SizeToString(Size) + " " + GetDestinationAsString() + ", " + GetSourceAsString();
            }
        }
	}
}