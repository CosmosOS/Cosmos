using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mul")]
	public class Multiply: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xE0,
                DestinationReg = Guid.Empty,
                OperandSizeByte=0,
                ReverseRegisters=true,
                DefaultSize=InstructionSize.Byte
            }); // EAX with register                
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x20,
                OperandSizeByte = 0,
                DestinationMemory = true,
                ReverseRegisters=true,
                DefaultSize = InstructionSize.Byte
            }); // EAX with register      
        }
	}
}
