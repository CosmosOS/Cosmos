using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler
{
    [OpCode("%else")]
    public class Else: Instruction
    {
        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(Mnemonic);
        }
    }
}
