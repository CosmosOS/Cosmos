using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("out")]
    public class Out : InstructionWithDestinationAndSize {
        public override void WriteText( Cosmos.Compiler.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            aOutput.Write(mMnemonic);
            aOutput.Write(" DX, ");
            aOutput.Write(this.GetDestinationAsString());
        }
	}
}