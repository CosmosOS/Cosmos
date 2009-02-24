using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("neg")]
	public class Neg: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0xF6},
                NeedsModRMByte=true,
                InitialModRMByteValue = 0xD8,
                ReverseRegisters= true,
                DestinationReg=Guid.Empty,
                OperandSizeByte=0
            }); // register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[]{0xF6},
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x18,
                ReverseRegisters = true,
                DestinationMemory = true,
                OperandSizeByte=0
            });
        }
	}
}