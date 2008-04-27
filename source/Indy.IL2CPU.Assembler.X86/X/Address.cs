using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Address {

        public static implicit operator Address(Register32 aRegister) {
            return new AddressIndirect(aRegister, 0);
        }

    }
}
