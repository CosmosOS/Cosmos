using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xFFFFFFFF, "rcl")]
    public class RotateThroughCarryLeft: Instruction
    {
        private readonly string mDestination;
        public RotateThroughCarryLeft(string aDestination) { mDestination = aDestination; }
        public override string ToString()
        {
            return "rcl " + mDestination + ", cl";
        }
    }
}