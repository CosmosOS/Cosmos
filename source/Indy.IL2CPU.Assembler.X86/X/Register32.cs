using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Register32 : Register {

        public static AddressIndirect operator +(Register32 aBaseRegister, UInt32 aDisplacement) {
            return new AddressIndirect(aBaseRegister, aDisplacement);
        }

    }
}
