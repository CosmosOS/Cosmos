using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class OpCodeAttribute : Attribute {
        public readonly uint OpCode;
        public readonly string Mnemonic;

        public OpCodeAttribute(uint aOpCode, string aMnemonic) {
            OpCode = aOpCode;
            Mnemonic = aMnemonic;
        }

    }
}
