using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xFFFFFFFF, "smsw")]
    public class smsw: Instruction
    {
        public readonly string Target;
        public smsw(string aTarget){
            Target = aTarget;
        }
        public override string ToString()
        {
            return "smsw " + Target;
        }
    }
}
