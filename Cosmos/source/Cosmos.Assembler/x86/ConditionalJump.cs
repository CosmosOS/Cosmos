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
            // MtW: NEVER EVER remove "near" here! It causes Nasm to take about 100 times as muh time for assembling....
            mMnemonic = "J" + Condition.GetMnemonic().ToUpperInvariant() + " near";
            base.WriteText(aAssembler, aOutput);
        }
    }
}
