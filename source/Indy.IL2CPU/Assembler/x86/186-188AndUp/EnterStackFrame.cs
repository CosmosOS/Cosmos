using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86._186_188AndUp
{
    [OpCode(0xFFFFFFFF, "enter")]
    public class EnterStackFrame: Instruction
    {
        public readonly string address1;
        public readonly string address2OrOpt;
        public EnterStackFrame(string address1, string address2OrOpt)
        {
            this.address1 = address1;
            this.address2OrOpt = address2OrOpt;
            

        }
        public override string ToString()
        {
            return "Enter "+address1 + ","+address2OrOpt;
        }

    }
}
