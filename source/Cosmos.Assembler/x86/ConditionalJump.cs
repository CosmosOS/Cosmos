using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    [Cosmos.Assembler.OpCode("jcc")]
    public class ConditionalJump: JumpBase, IInstructionWithCondition {
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText( Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            mMnemonic = String.Intern("j" + Condition.GetMnemonic() + " near");
            base.WriteText(aAssembler, aOutput);
        }
    }
}