using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.x87
{
    [OpCode("fdiv")]
    public class FloatDivide : InstructionWithDestinationAndSourceAndSize
    {
    }
}