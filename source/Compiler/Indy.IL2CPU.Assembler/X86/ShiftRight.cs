using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("shr")]
	public class ShiftRight: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xD2 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xE8,
                OperandSizeByte = 0,
                ReverseRegisters = true,
                DestinationReg = Guid.Empty,
                SourceReg = Registers.CL,
                SourceRegByte = -1
            }); // register by CL
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xD2 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x28,
                ReverseRegisters = true,
                OperandSizeByte = 0,
                DestinationMemory = true,
                SourceReg = Registers.CL,
                SourceRegByte = -1
            }); // memory by CL
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC0 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xE8,
                OperandSizeByte = 0,
                DestinationReg = Guid.Empty,
                ReverseRegisters = true,
                SourceImmediate = true,
                SourceImmediateSize = InstructionSize.Byte
            }); // register by immediate
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xC0 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x28,
                OperandSizeByte = 0,
                ReverseRegisters = true,
                DestinationMemory = true,
                SourceImmediate = true,
                SourceImmediateSize = InstructionSize.Byte
            }); // memory by immediate
        }
	}
}