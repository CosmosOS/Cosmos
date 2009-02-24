using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("stosw")]
	public class StoreWordInString: Instruction {
#warning todo: merge with StoreByteInString, and inherit from InstructionWithSize, like what's done with RepeatMovs
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x66, 0xAB }
            });
        }
	}
}