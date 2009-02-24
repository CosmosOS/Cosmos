using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86
{
	/// <summary>
	/// Subtracts the source operand from the destination operand and 
	/// replaces the destination operand with the result. 
	/// </summary>
    [OpCode("sbb")]
	public class SubWithCarry : InstructionWithDestinationAndSourceAndSize
	{
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x18 },
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xC0,
                DestinationReg = Guid.Empty,
                SourceReg = Guid.Empty,
                OperandSizeByte = 0,
                ReverseRegisters = true
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x1A },
                NeedsModRMByte = true,
                SourceMemory = true,
                DestinationReg = Guid.Empty,
                OperandSizeByte = 0
            }); // mem to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x18 },
                NeedsModRMByte = true,
                DestinationMemory = true,
                SourceReg = Guid.Empty,
                OperandSizeByte = 0,
                ReverseRegisters = true
            }); // reg to mem
        }
	}
}
