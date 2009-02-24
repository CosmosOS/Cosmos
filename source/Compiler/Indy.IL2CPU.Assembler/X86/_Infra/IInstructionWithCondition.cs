using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public interface IInstructionWithCondition {
        ConditionalTestEnum Condition {
            get;
            set;
        }
    }
}
