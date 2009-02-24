using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("dec")]
    public class Dec : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[]{0xFE},
                NeedsModRMByte = true,
                InitialModRMByteValue=0x8,
                OperandSizeByte=0,
                DestinationMemory=true,
                ReverseRegisters=true
            }); //memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[]{0x48},
                DestinationReg=Guid.Empty,
                DestinationRegByte = 0,
                AllowedSizes=InstructionSizes.DWord | InstructionSizes.Word
            }); // reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFE },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC8,
                OperandSizeByte = 0,
                ReverseRegisters=true,
                DestinationReg = Guid.Empty
            }); // reg
        }
    }
}
