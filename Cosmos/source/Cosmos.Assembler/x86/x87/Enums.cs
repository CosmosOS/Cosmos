using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86.x87
{
    public enum FloatConditionalMoveTestEnum : byte
    {
        Below = 0,
        Equal = 1,
        BelowOrEqual = 2,
        Unordered = 3,
        NotBelow = 4,
        NotEqual = 5,
        NotBelowOrEqual = 6,
        Ordered = 7
    }
}