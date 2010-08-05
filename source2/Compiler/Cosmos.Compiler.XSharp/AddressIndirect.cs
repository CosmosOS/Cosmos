using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.X {
    public class AddressIndirect : Address {
        public readonly RegistersEnum? Register;
        public readonly ElementReference Reference;
        public readonly uint Address;
        public readonly int Displacement;

        public AddressIndirect(Register32 aBaseRegister, Int32 aDisplacement) {
            Register = Registers.GetRegister(aBaseRegister.Name);
            Displacement = aDisplacement;
        }
        public AddressIndirect(uint aBaseAddress, int aDisplacement) {
            Address = aBaseAddress;
            Displacement = aDisplacement;
        }

        public override string ToString() {
            if (Register != null) {
                if (Displacement == 0) {
                    return "[" + Registers.GetRegisterName(Register.Value) + "]";
                } else {
                    return "[" + Registers.GetRegisterName(Register.Value) + " + " + Displacement + "]";
                }
            } else {
                if (Displacement == 0) {
                    return "[" + Address.ToString() + "]";
                } else {
                    return "[" + Address.ToString() + " + " + Displacement + "]";
                }
            }
        }
    }
}
