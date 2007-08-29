using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler {
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class OpCodeAttribute : Attribute {
        private readonly uint mOpCode;
        public OpCodeAttribute(uint aOpCode) {
            mOpCode = aOpCode;
        }

        public uint OpCode {
            get { return mOpCode; }
        }
    }
}
