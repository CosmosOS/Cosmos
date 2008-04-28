using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class MemoryAction : Action {

        // This form used for reading memory - Addresses are passed in
        public MemoryAction(string aValue) {
            mValue = aValue;
        }

    }
}
