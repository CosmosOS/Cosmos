using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0xFFFFFFFF, "dec")]
    public class Dec : InstructionWithDestinationAndSize {
        public override string ToString() {
            if (Size>0) {
                return "dec " + SizeToString(Size) + " " + GetDestinationAsString();
            } else {
                return "dec " + GetDestinationAsString();
            }
        }
    }
}
