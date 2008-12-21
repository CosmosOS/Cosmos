using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("push")]
    public class Push : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x50 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 0,
                DestinationRegBitShiftLeft = 0,
                ReverseRegisters=true,
                DefaultSize = InstructionSize.DWord,
                AllowedSizes=InstructionSizes.DWord | InstructionSizes.Word
            }); // register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x6A },
                DestinationImmediate = true,
                DefaultSize = InstructionSize.DWord,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            }); // immediate
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF },
                NeedsModRMByte = true,
                DestinationMemory = true,
                InitialModRMByteValue = 0x30,
                ReverseRegisters = true,
                DefaultSize = InstructionSize.DWord,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            }); // pop to memory
        }

        public Push() {
            Size = 32;
        }
    }
}
