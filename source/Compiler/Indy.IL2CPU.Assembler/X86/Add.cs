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
                OpCode = new byte[] { 0x80 },
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC0,
                OperandSizeByte = 0,
                DestinationReg = Guid.Empty,
                SourceImmediate = true
            }); // immediate to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x00 },
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word | InstructionSizes.Byte,
                DefaultSize = InstructionSize.DWord,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC0,
                ReverseRegisters=true,
                OperandSizeByte = 0,
                DestinationReg = Guid.Empty,
                SourceReg = Guid.Empty,
            }); // register to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                NeedsModRMByte = true,
                OperandSizeByte = 0,
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord,
                DestinationMemory = true,
                SourceImmediate = true
            }); // immediate to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x00 },
                NeedsModRMByte = true,
                OperandSizeByte = 0,
                ReverseRegisters=true,
                AllowedSizes = InstructionSizes.Byte | InstructionSizes.Word | InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord,
                DestinationMemory = true,
                SourceReg = Guid.Empty
            }); // register to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[]{0x02},
                NeedsModRMByte = true,
                //ReverseRegisters=true,
                OperandSizeByte=0,
                DestinationReg=Guid.Empty,
                SourceMemory=true
            }); // memory to register
        }
	}
}