using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class ConstantAction : Action {
        public ConstantAction(UInt32 aValue) {
            mValue = aValue.ToString();
        }
    }
}
