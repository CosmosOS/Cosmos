using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("cmp")]
	public class Compare: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x38},
                OperandSizeByte=0,
                NeedsModRMByte=true,
                InitialModRMByteValue=0xC0,
                DestinationRegAny=true,
                SourceRegAny = true,
                ReverseRegisters=true
            }); // reg with reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x3A },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                DestinationRegAny = true,
                SourceMemory = true
            }); // memory with reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x38 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                DestinationMemory = true,
                ReverseRegisters=true,
                SourceRegAny = true
            }); // reg with memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x3C },
                OperandSizeByte = 0,
                DestinationReg = Registers.EAX,
                SourceImmediate = true,
                ReverseRegisters = true
            }); // immediate with EAX/AX/AL
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte = true,
                InitialModRMByteValue=0xF8,
                DestinationRegAny = true,
                SourceImmediate = true,
                ReverseRegisters=true
            }); // immediate with register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x80 },
                OperandSizeByte = 0,
                NeedsModRMByte=true,
                InitialModRMByteValue=0x38,
                DestinationMemory=true,
                SourceImmediate = true,
                ReverseRegisters=true
            }); // immediate with memory
        }
	}
}