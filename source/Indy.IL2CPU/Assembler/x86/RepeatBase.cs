using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    public abstract class RepeatBase: Instruction {
        public readonly string instruction;

        protected RepeatBase(string instruction) {
            this.instruction = instruction;
        }

    }
}
