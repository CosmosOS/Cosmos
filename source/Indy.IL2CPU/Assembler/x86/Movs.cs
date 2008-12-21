using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("movs")]
    public class Movs: Instruction, IInstructionWithSize, IInstructionWithPrefix {
        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public byte Size {
            get;
            set;
        }
    }
}