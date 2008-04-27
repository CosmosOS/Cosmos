using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class AddressIndirect : Address {
        protected Register32 mBaseRegister;
        protected UInt32 mDisplacement = 0;

        public AddressIndirect(Register32 aBaseRegister, UInt32 aDisplacement) {
            mBaseRegister = aBaseRegister;
            mDisplacement = aDisplacement;
        }

        public override string ToString() {
            if (mDisplacement == 0) {
                return "[" + mBaseRegister.ToString() + "]";
            } else {
                return "[" + mBaseRegister.ToString() + " + " + mDisplacement + "]";
            }
        }
    }
}
