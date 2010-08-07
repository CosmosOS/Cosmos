using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Assembler;
using Cosmos.Compiler.Assembler.X86;

namespace Cosmos.Compiler.XSharp {
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