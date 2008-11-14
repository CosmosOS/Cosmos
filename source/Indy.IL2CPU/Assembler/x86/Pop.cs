using System;
using System.Linq;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("pop")]
	public class Pop: InstructionWithDestination{
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                AllowedSizes = InstructionSizes.DWord,
                OpCode = new byte[] { 0x58 },
                NeedsModRMByte = false,
                DestinationReg = Guid.Empty,
                DestinationRegByte = 0
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                AllowedSizes =  InstructionSizes.DWord,
                OpCode = new byte[]{0x8F},
                NeedsModRMByte=true,
                DestinationMemory=true
            });
        }
        public override string ToString()
        {
            return base.mMnemonic + " dword " + this.GetDestinationAsString();
        }
	}

}