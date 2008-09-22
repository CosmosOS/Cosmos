using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode(0x000000, "jecxz")]
    public class JumpIfECXZero : JumpBase {
        public JumpIfECXZero(string aAddress) :base(aAddress){
        }

        public override string ToString()
        {
            return "jecxz " + Address;
        }
    }
}