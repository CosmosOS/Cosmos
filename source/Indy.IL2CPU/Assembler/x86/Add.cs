using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("add")]
	public class Add: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x04 },
                AllowedSizes = InstructionSizes.Word | InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord,
                OperandSizeByte = 0,
                DestinationReg = Registers.EAX,
                SourceImmediate = true
            }); // immediate to reg, shortcut for EAX/AX/AL
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80, 0xC0 },
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord,
                OperandSizeByte=0,
                DestinationReg=Guid.Empty,
                DestinationRegByte = 1,
                SourceImmediate=true
            }); // immediate to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0, 0xC0 },
                AllowedSizes=InstructionSizes.DWord,
                DefaultSize=InstructionSize.DWord,
                OperandSizeByte = 0,
                DestinationReg = Guid.Empty,
                DestinationRegByte = 1,
                SourceReg = Guid.Empty,
                SourceRegByte = 1,
                SourceRegBitShiftLeft = 3
            });
        }
	}
}