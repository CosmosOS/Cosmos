using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("not")]
	public class Not: InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                DestinationMemory = true,
                NeedsModRMByte=true,
                DefaultSize=InstructionSize.DWord,
                OperandSizeByte=0,
                ReverseRegisters = true,
                InitialModRMByteValue = 0x10
            }); // memory
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF6 },
                DestinationReg = Guid.Empty,
                NeedsModRMByte = true,
                InitialModRMByteValue = 0xD0,
                ReverseRegisters = true,
                OperandSizeByte=0
            }); // register
        }
	}
}
