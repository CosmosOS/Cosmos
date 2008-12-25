using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("or")]
    public class Or : InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0A },
                NeedsModRMByte = true,
                OperandSizeByte = 0,
                SourceMemory = true,
                DestinationReg = Guid.Empty
            }); // memory to register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x08 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC0,
                DestinationReg = Guid.Empty,
                SourceReg = Guid.Empty,
                ReverseRegisters = true,
                OperandSizeByte = 0
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0x08,
                SourceImmediate = true,
                DestinationMemory = true,
                ReverseRegisters = true
            }); // immediate to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x08 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                SourceReg = Guid.Empty,
                DestinationMemory = true,
                ReverseRegisters = true
            }); // reg to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0C },
                OperandSizeByte = 0,
                SourceImmediate = true,
                DestinationReg = Registers.EAX,
                ReverseRegisters = true
            }); // immediate to reg (eax)
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC8,
                SourceImmediate = true,
                DestinationReg = Guid.Empty,
                ReverseRegisters = true
            }); // immediate to reg
        }
    }
}