using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("cmpxchg")]
	public class CmpXchg: InstructionWithDestinationAndSourceAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[]{0x0F, 0xB0},
                OperandSizeByte=1,
                NeedsModRMByte=true,
                DestinationMemory=true,
                SourceReg=Guid.Empty,
                ReverseRegisters=true
            }); // register to memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x0F, 0xB0},
                OperandSizeByte=1,
                NeedsModRMByte=true,
                ReverseRegisters=true,
                InitialModRMByteValue = 0xC0,
                SourceReg=Guid.Empty,
                DestinationReg=Guid.Empty
            }); // register1, register2
        }
	}
}