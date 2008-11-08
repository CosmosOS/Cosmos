using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86 = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Register08 : Register {
        public void Compare(byte aValue) {
            new Compare { DestinationReg = GetId(), SourceValue = aValue, Size = 8 };
        }

        public void Test(byte aValue) {
            new X86.Test { DestinationReg = GetId(), SourceValue = aValue, Size = Registers.GetSize(GetId()) };
        }
    }
}
