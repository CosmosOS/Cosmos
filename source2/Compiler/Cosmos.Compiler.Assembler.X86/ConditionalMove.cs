using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("cmovcc")]
    public class ConditionalMove: InstructionWithDestinationAndSourceAndSize, IInstructionWithCondition {
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            mMnemonic = "cmov" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }
}