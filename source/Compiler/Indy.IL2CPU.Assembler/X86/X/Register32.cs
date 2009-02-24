using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Register32 : Register {

        public static AddressIndirect operator +(Register32 aBaseRegister, Int32 aDisplacement) {
            return new AddressIndirect(aBaseRegister, aDisplacement);
        }

        // This syntax would be nice:
        // EBP = EBP + 32
        // But it would conflict with C#'s resolution of [EBP + 4] becuase
        // C# on operator overloads does not look at return type, only argument types
        public void Add(UInt32 aValue) {
            new Add { DestinationReg = GetId(), SourceValue=aValue };
        }

        public void Compare(UInt32 aValue) {
            new Compare { DestinationReg = GetId(), SourceValue = aValue };
        }

        public void Test(UInt32 aValue) {
            new X86.Test { DestinationReg = GetId(), SourceValue = aValue, Size = 32 };
        }
    }
}