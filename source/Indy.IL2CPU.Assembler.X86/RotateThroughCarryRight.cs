using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xFFFFFFFF, "rcr")]
    public class RotateThroughCarryRight: Instruction
    {
        private readonly string mDestination;
        public RotateThroughCarryRight(string aDestination) { mDestination = aDestination; }
        public override string ToString()
        {
            return "rcr " + mDestination + ", cl";
        }
    }
}