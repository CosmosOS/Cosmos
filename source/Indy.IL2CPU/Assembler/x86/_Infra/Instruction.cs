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

        protected static string SizeToString(byte aSize) {
            switch (aSize) {
                case 8:
                    return "byte";
                case 16:
                    return "word";
                case 32:
                    return "dword";
                case 64:
                    return "qword";
                default:
                    throw new Exception("Invalid size: " + aSize);
            }
        }
    }

    // todo: remove when transition is done
    public abstract class New_Instruction : Instruction {
        protected New_Instruction() { }
    }
}
