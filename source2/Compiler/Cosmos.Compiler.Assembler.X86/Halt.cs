using System;
using System.Linq;

namespace Cosmos.Compiler.Assembler.X86
{
    [OpCode("hlt")]
    public class Halt : Instruction
    {
    }
}