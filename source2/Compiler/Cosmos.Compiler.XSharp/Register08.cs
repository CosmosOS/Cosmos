using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Cosmos.IL2CPU.X86;

namespace Cosmos.IL2CPU.X86.X {
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
