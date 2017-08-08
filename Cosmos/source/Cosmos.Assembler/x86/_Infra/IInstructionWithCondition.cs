using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public interface IInstructionWithCondition {
        ConditionalTestEnum Condition {
            get;
            set;
        }
    }
}
