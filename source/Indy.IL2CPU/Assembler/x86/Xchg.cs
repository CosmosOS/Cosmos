using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0xFFFFFFFF, "xchg")]
    public class Xchg : Instruction {
        public readonly string Destination;
        public readonly string Source;
        public Xchg(string aDestination, string aSource) {
            Destination = aDestination;
            Source = aSource;
        }

        public override string ToString() {
            return "xchg " + Destination + ", " + Source;
        }
    }
}
