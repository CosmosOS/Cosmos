using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("mov")]
	public class Move: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            //aData.DefaultSize = InstructionSize.DWord;
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xB0 },
                //NeedsModRMByte=true,
                DefaultSize = InstructionSize.DWord,
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                OperandSizeByte = 0,
                OperandSizeBitShiftLeft = 3,
                DestinationReg = Guid.Empty,
                DestinationRegByte = 0,
                SourceImmediate = true
            });  // immediate to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC6 },
                NeedsModRMByte = true,
                AllowedSizes = InstructionSizes.Word,
                OperandSizeByte = 0,
                OperandSizeBitShiftLeft = 0,
                SourceImmediate = true,
                //DestinationRegByte = 1,
                DestinationMemory = true,
                DestinationReg = Guid.Empty
            });  // immediate to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x88, 0xC0 },
                OperandSizeByte = 0,
                SourceReg=Guid.Empty,
                SourceRegByte=1,
                SourceRegBitShiftLeft = 3,
                DestinationRegByte = 1,
                DestinationReg=Guid.Empty
            }); // register to register
        }
	}
}
