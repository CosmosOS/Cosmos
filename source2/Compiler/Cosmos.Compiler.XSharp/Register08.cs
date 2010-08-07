using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
    public class Register08 : Register {
        public Register08() {
            mBitSize = 8;
        }

        public void Compare(byte aValue) {
            new Compare { DestinationReg = GetId(), SourceValue = aValue, Size = 8 };
        }

        public void Test(byte aValue) {
            new Test { DestinationReg = GetId(), SourceValue = aValue, Size = Registers.GetSize(GetId()) };
        }
    }
}
