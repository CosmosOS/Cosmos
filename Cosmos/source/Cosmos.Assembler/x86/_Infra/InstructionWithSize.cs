using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Assembler.x86 {
    public abstract class InstructionWithSize: Instruction, IInstructionWithSize {
        protected InstructionWithSize() {
            
        }

        public byte Size {
            get;
            set;
        }
    }
}
