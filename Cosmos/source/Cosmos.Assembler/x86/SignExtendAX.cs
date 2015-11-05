using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("cdq")]
    public class SignExtendAX : InstructionWithSize {
        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            switch (Size) {
                case 32:
                    aOutput.Write("cdq");
                    return;
                case 16:
                    aOutput.Write("cwde");
                    return;
                case 8:
                    aOutput.Write("cbw");
                    return;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}