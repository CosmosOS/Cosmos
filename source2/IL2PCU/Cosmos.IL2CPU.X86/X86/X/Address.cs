using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class Address {

        public static implicit operator Address(Register32 aRegister) {
            return new AddressIndirect(aRegister, 0);
        }

        public static implicit operator Address(UInt32 aAddress) {
            return new AddressIndirect(aAddress, 0);
        }

        public static implicit operator Address(string aLabel) {
            return new AddressDirect(aLabel);
        }

    }
}
