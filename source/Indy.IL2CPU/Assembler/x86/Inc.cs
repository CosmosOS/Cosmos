using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0xFFFFFFFF, "inc")]
    public class Inc : InstructionWithDestinationAndSize {
        public override string ToString() {
            if (Size > 0) {
                return "inc " + SizeToString(Size) + " " + GetDestinationAsString();
            } else {
                return "inc " + GetDestinationAsString();
            }
        }
    }
}
