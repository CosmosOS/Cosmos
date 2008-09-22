using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0xFFFFFFFF, "inc")]
    public class Inc : Instruction {
        public readonly string Destination;
        public readonly string Size;

        public Inc(string aSize, string aDestination) {
            Size = aSize;
            Destination = aDestination;
        }

        public Inc(string aDestination) {
            Destination = aDestination;
        }

        public override string ToString() {
            if (String.IsNullOrEmpty(Size)) {
                return "inc " + Destination;
            } else {
                return "inc " + Size + " " + Destination;
            }
        }
    }
}
