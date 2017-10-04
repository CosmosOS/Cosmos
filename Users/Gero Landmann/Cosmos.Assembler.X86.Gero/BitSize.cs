using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    [Flags]
    public enum BitSize : int
    {
        None,
        Bits8 = 8,
        Bits16 = 16,
        Bits32 = 32,
        Bits64 = 64,
        Bits80 = 80,
        Bits128 = 128,
        Bits256 = 256,
        Default = Bits32,
    }
}
