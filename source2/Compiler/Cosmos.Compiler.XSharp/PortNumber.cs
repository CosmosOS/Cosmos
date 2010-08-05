using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86.X {
    public class PortNumber {
        public readonly RegistersEnum Register;

        public PortNumber(RegistersEnum aRegister)
        {
            Register = aRegister;
        }

        public override string ToString() {
            return Registers.GetRegisterName(Register);
        }
    }
}