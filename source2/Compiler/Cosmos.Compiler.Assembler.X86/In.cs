using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("in")]
    public class In : InstructionWithDestinationAndSize {
        public override void WriteText( Cosmos.Compiler.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            base.WriteText(aAssembler, aOutput);
            aOutput.Write(", DX");
        }
    }
}
