using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("xor")]
	public class Xor: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x32 },
                NeedsModRMByte = true,
                OperandSizeByte = 0,
                SourceMemory = true,
                DestinationRegAny = true
            }); // memory to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x30 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC0,
                DestinationRegAny = true,
                SourceRegAny = true,
                ReverseRegisters = true,
                OperandSizeByte = 0
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x30,
                SourceImmediate = true,
                DestinationMemory = true,
                ReverseRegisters = true
            }); // immediate to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x30 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                SourceRegAny = true,
                DestinationMemory = true,
                ReverseRegisters = true
            }); // reg to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x34 },
                OperandSizeByte = 0,
                SourceImmediate = true,
                DestinationReg = Registers.EAX,
                ReverseRegisters = true
            }); // immediate to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xF0,
                SourceImmediate = true,
                DestinationRegAny = true,
                ReverseRegisters = true
            }); // immediate to reg
        }
	}
}
