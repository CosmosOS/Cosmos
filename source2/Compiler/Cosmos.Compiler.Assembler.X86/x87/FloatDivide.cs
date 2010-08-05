using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.x87
{
    [OpCode("fdiv")]
    public class FloatDivide : InstructionWithDestinationAndSourceAndSize
    {
        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {

        }
    }
}