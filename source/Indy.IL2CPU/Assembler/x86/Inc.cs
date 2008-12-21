using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("inc")]
    public class Inc : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x40},
                DestinationReg=Guid.Empty,
                DestinationRegByte=0,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            }); // reg (alt)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFE },
                DestinationReg = Guid.Empty,
                NeedsModRMByte=true,
                InitialModRMByteValue=0xC0,
                ReverseRegisters=true,
                OperandSizeByte=0
            }); // reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFE },
                DestinationMemory=true,
                NeedsModRMByte = true,
                ReverseRegisters = true,
                OperandSizeByte = 0
            }); // memory
        }
    }
}
