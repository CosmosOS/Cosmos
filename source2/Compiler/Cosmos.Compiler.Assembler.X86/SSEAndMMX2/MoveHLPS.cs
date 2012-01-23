using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.SSE
{
    [Cosmos.Assembler.OpCode("movhlps")]
    public class MoveHLPS : InstructionWithDestinationAndSource
    {
    }
}
