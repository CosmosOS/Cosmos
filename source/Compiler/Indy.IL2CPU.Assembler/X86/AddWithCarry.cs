using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86
{
    [OpCode("adc")]
	public class AddWithCarry : InstructionWithDestinationAndSourceAndSize
	{
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[]{0x10},
                NeedsModRMByte=true,
                InitialModRMByteValue=0xC0,
                DestinationReg=Guid.Empty,
                SourceReg=Guid.Empty,
                OperandSizeByte=0,
                ReverseRegisters=true
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x12},
                NeedsModRMByte=true,
                SourceMemory=true,
                DestinationReg=Guid.Empty,
                OperandSizeByte=0
            }); // mem to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x10 },
                NeedsModRMByte = true,
                DestinationMemory = true,
                SourceReg = Guid.Empty,
                OperandSizeByte = 0,
                ReverseRegisters=true
            }); // reg to mem
        }
	}
}
