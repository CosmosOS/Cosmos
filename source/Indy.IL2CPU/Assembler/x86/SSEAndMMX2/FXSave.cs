using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("fxsave")]
	public class FXSave : InstructionWithDestination {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0xAE },
                NeedsModRMByte = true,
                DestinationMemory = true,
                ReverseRegisters=true
            });
        }
	}
}
