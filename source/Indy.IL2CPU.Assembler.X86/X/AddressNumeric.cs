using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class AddressNumeric : Address {
        protected UInt32 mAddress;

        public AddressNumeric(UInt32 aAddress) {
            mAddress = aAddress;
        }

        public override string ToString() {
          return mAddress.ToString();
        }

        public static implicit operator AddressNumeric(UInt32 aAddress) {
            return new AddressNumeric(aAddress);
        }
    }
}
