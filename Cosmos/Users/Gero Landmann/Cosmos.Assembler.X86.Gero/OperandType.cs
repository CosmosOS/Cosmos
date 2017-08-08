using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    [Flags]
    public enum OperandType
    {
        Register,/* register number in 'basereg' */
        Immediate,
        Memory,
        RegMem,/* for r/m, ie EA, operands */
    }
 
}
