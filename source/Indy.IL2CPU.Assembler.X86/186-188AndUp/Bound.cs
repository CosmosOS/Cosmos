using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86._186_188AndUp
{
    [OpCode(0xFFFFFFFF, "Bound")]
    public class Bound : Instruction
    {
        public readonly string index;
        public readonly string operand;

        public Bound(string index, string operand)
        {
            this.index = index;
            this.operand = operand;
        }

        public override string ToString()
        {
            return "bound " + index + "," + operand;
        }
    }
}
