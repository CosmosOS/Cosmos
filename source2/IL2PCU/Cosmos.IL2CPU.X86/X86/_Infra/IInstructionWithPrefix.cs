using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    [Flags]
    public enum InstructionPrefixes {
        None,
        Lock,
        Repeat,
        RepeatNotEqual
    }

    public interface IInstructionWithPrefix {
        InstructionPrefixes Prefixes {
            get;
            set;
        }
    }
}
