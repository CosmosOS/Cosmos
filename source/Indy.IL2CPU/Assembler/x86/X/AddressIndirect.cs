using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class AddressIndirect : Address {
        protected Register32 mBaseRegister;
        protected UInt32 mDisplacement = 0;
        protected uint mBaseAddress;

        public AddressIndirect(Register32 aBaseRegister, UInt32 aDisplacement) {
            mBaseRegister = aBaseRegister;
            mDisplacement = aDisplacement;
        }
        public AddressIndirect(uint aBaseAddress, uint aDisplacement) {
            mBaseAddress = aBaseAddress;
            mDisplacement = aDisplacement;
        }

        public override string ToString() {
            if (mBaseRegister != null) {
                if (mDisplacement == 0) {
                    return "[" + mBaseRegister.ToString() + "]";
                } else {
                    return "[" + mBaseRegister.ToString() + " + " + mDisplacement + "]";
                }
            } else {
                if (mDisplacement == 0) {
                    return "[" + mBaseAddress.ToString() + "]";
                } else {
                    return "[" + mBaseAddress.ToString() + " + " + mDisplacement + "]";
                }
            }
        }
    }
}
