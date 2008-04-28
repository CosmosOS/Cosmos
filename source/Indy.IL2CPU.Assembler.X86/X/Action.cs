using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Action {
        protected string mValue;

        public override string ToString() {
            return mValue;
        }

        public static implicit operator Action(UInt32 aValue) {
            return new ConstantAction(aValue);
        }
    }
}
