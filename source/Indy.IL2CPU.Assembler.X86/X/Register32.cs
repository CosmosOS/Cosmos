using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Register32 : Register {

        public static AddressIndirect operator +(Register32 aBaseRegister, UInt32 aDisplacement) {
            return new AddressIndirect(aBaseRegister, aDisplacement);
        }

        // This syntax would be nice:
        // EBP = EBP + 32
        // But it would conflict with C#'s resolution of [EBP + 4] becuase
        // C# on operator overloads does not look at return type, only argument types
        public void Add(UInt32 aValue) {
            new Add(ToString(), aValue);
        }

    }
}
