using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86.SSE
{
    public enum ComparePseudoOpcodes : byte
    {
        Equal = 0,
        LessThan = 1,
        LessThanOrEqualTo = 2,
        Unordered = 3,
        NotEqual = 4,
        NotLessThan = 5,
        NotLessThanOrEqualTo = 6,
        Ordered = 7
    }
}
