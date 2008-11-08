using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("int")]
    public class Interrupt : Instruction {
        public readonly string Number;

        /// <summary>
        /// Interrupt N4. Indicates integer overflow.
        /// </summary>
        public const int INTO = 4;

        public Interrupt(int aNumber)
            : this(aNumber.ToString()) {
        }

        public Interrupt(string aNumber) {
            Number = aNumber;
        }

        public override string ToString() {
            //if (Number == INTO) return "into";
            return "int " + Number;
        }
    }
}