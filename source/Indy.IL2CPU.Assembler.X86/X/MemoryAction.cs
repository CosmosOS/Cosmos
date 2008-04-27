using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class MemoryAction {
        protected string mValue;

        public MemoryAction(string aValue) {
            mValue = aValue;
        }

        public override string ToString() {
            return mValue;
        }
    }
}
