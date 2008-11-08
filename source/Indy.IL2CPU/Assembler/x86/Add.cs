using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
	[OpCode(0xFFFFFFFF, "add")]
	public class Add: InstructionWithDestinationAndSourceAndSize {
        public override string ToString() {
            if (Size == 0) {
                return "add " + GetDestinationAsString() + ", " + GetSourceAsString();
            } else {
                return "add " + SizeToString(Size) + " " + GetDestinationAsString() + ", " + GetSourceAsString();
            }
        }
	}
}