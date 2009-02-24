using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class InstructionWithSize: Instruction, IInstructionWithSize {
        protected InstructionWithSize() {
            
        }

        public byte Size {
            get;
            set;
        }

        public override abstract string ToString();
    }
}
