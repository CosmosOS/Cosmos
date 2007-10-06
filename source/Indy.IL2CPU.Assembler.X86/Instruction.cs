using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
    	protected Instruction() {
            // This is done this way so opcode and mnemonic can be easily specified in actual class
            // Yet overriden later if necessary as some instructions have seperate mnemonics on some platforms for simple variations,
            // and other platforms have one mnemonic and opcode changes based on params.
    	}

        // If there are params, descendants should override this and return Mnemonic + params

    	// This is virtual and not abstract so that opcodes like Noop dont need to override it 
        // since they do not have any params.
        public virtual void EmitParams(BinaryWriter aWriter) {
        }
    }
}
