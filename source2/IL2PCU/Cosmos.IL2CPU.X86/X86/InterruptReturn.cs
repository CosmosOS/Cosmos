using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    [OpCode("iret")]
	public class InterruptReturn: Instruction {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xCF }
            });
        }
	}
}