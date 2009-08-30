using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.X86
{
    [Flags]
    public enum ImmediateSize : int
    {
        None,
        Byte = 8,
        Word = 16,
        DWord = 32,
        QWord = 64,
        TWord = 80,
        DQWord = 128,
        YWord = 256,
        Default = DWord,
    }
}
