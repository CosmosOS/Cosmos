using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class PortNumber {
        public readonly byte Port;
        public readonly Guid Register;

        public PortNumber(byte aPort) {
            Port = aPort;
        }

        public PortNumber(Guid aRegister) {
            Register = aRegister;
        }

        public override string ToString() {
            if (Register == Guid.Empty) {
                return Port.ToString();
            } else {
                return Registers.GetRegisterName(Register);
            }
        }

    }
}
