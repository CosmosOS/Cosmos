using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.X {
    public class Memory {
        public MemoryAction this[Address aAddress] {
            get {
                return new MemoryAction(aAddress.ToString());
            }
            set {
            }
        }
    }
}
