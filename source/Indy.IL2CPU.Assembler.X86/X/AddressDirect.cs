using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class AddressDirect : Address {
        protected readonly string mAddress;

        public AddressDirect(UInt32 aAddress) {
            mAddress = aAddress.ToString();
        }

        public AddressDirect(string aLabel) {
            mAddress = "[" + aLabel + "]";
        }

        public override string ToString() {
          return mAddress.ToString();
        }

    }
}
