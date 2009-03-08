using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86.ELF {
    public abstract class BaseSection : BaseDataStructure {
        public abstract uint MemorySize {
            get;
        }

        public abstract uint MemoryOffset {
            get;
        }
    }
}
