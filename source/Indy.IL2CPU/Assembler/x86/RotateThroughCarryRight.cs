using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode(0xFFFFFFFF, "rcr")]
    public class RotateThroughCarryRight: InstructionWithDestinationAndSize
    {
        public override string ToString()
        {
            return base.ToString() + ", CL";
        }
    }
}