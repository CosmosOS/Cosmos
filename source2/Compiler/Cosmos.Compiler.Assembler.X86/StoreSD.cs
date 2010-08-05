using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86
{
    [OpCode("stosd")]
    public class StoreSD : Instruction
    {
#warning todo: merge with stosb and stosw
    }
}
