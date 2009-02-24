using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class PortNumber {
        public readonly Guid Register;

        public PortNumber(Guid aRegister) {
            Register = aRegister;
        }

        public override string ToString() {
            return Registers.GetRegisterName(Register);
        }
    }
}