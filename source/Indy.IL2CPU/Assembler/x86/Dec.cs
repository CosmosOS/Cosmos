using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0xFFFFFFFF, "dec")]
    public class Dec : Instruction {
        public readonly string Destination;
        public readonly string Size;

        public Dec(string aSize, string aDestination) {
            Size = aSize;
            Destination = aDestination;
        }

        public Dec(string aDestination) {
            Destination = aDestination;
        }

        public override string ToString() {
            if (String.IsNullOrEmpty(Size)) {
                return "dec " + Destination;
            } else {
                return "dec " + Size + " " + Destination;
            }
        }
    }
}
