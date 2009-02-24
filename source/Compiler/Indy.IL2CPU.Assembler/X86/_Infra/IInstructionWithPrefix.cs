using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [Flags]
    public enum InstructionPrefixes {
        None,
        Lock,
        Repeat
    }

    public interface IInstructionWithPrefix {
        InstructionPrefixes Prefixes {
            get;
            set;
        }
    }
}
