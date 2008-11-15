using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("push")]
    public class Push : InstructionWithDestination{
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
OpCode = new byte[] {0x50},
AllowedSizes = InstructionSizes.DWord,
DestinationReg=Guid.Empty,
DestinationRegByte=0,
DestinationRegBitShiftLeft=0
            }); // register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x68 },
                AllowedSizes = InstructionSizes.DWord,
                DestinationImmediate=true
            }); // immediate

        }

        public Push()
        {
            //Changed without size
            //Size = 32;
        }
        public override string ToString()
        {
            return this.mMnemonic + " dword " + this.GetDestinationAsString();
        }


   }
}
