using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public class ConditionalJump: JumpBase, IInstructionWithCondition {
        public ConditionalTestEnum Condition {
            get;
            set;
        }
        public override string ToString() {
            mMnemonic = "j" + Condition.GetMnemonic() + " near";
            return base.ToString();
        }
    }
}