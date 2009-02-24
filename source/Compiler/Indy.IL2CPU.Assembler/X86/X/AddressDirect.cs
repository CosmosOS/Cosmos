using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class AddressDirect : Address {
        public readonly string Label;
        public readonly uint Address;
        public readonly Guid Register;

        public AddressDirect(Guid aRegister) {
            Register = aRegister;
        }

        public AddressDirect(UInt32 aAddress) {
            Address= aAddress;
        }

        public AddressDirect(string aLabel) {
            Label = aLabel;
        }

        public override string ToString() {
            if (Label == null) {
                return Address.ToString();
            } else {
                if (Register != Guid.Empty) {
                    return Registers.GetRegisterName(Register);
                }
                return Label;
            }
        }

    }
}
