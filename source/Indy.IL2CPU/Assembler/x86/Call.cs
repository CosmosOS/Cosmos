using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("call")]
	public class Call: JumpBase {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xE8 },
                DestinationImmediate = true,
                AllowedSizes = InstructionSizes.DWord
            }); // direct value
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF, 0xD0 },
                DestinationReg = Guid.Empty,
                DestinationRegByte=1,
                AllowedSizes = InstructionSizes.DWord
            }); // register indirect
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF },
                NeedsModRMByte=true,
                InitialModRMByteValue=0x10,
                DestinationMemory=true,
                ReverseRegisters=true,
                AllowedSizes = InstructionSizes.DWord
            }); // memory indirect
        }

        public Call() {
            mNear = false;
        }
	}
}