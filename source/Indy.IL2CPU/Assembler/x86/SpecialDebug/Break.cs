//#define Bochs_Magic_Breakpoint

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("")]
	public class Break: X86.Instruction {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[0]
            });
        }
		public Break() {
		}

		public override string ToString() {
#if Bochs_Magic_Breakpoint
			return "xchg bx, bx";
#else
			return "; Debug?";
#endif
		}
	}
}