using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler
{
    [Cosmos.Assembler.OpCode("%endif")]
    public class EndIfDefined : Instruction, IEndIfDefined {
        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            aOutput.Write(this.GetAsText());
        }
    }
}