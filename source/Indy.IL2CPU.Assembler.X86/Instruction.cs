using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    public abstract class Instruction : Indy.IL2CPU.Assembler.Instruction {
        protected uint mOpCode;
        public uint OpCode {
            get { return mOpCode; }
        }

        protected string mMnemonic;
        public string Mnemonic {
            get { return mMnemonic; }
        }

        protected Instruction() {
            // This is done this way so opcode and mnemonic can be easily specified in actual class
            // Yet overriden later if necessary as some instructions have seperate mnemonics on some platforms for simple variations,
            // and other platforms have one mnemonic and opcode changes based on params.
            var xAttrib = (OpCodeAttribute)(GetType().GetCustomAttributes(typeof(OpCodeAttribute), false)[0]);
            mOpCode = xAttrib.OpCode;
            mMnemonic = xAttrib.Mnemonic;
        }

        // If there are params, descendants should override this and return Mnemonic + params
        public override string ToString() {
			return Mnemonic;
        }

        // This is virtual and not abstract so that opcodes like Noop dont need to override it 
        // since they do not have any params.
        public virtual void EmitParams(BinaryWriter aWriter) {
        }
    }
}
