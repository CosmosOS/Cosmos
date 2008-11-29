using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("push")]
    public class Push : InstructionWithDestinationAndSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x50 },
                DestinationReg = Guid.Empty,
                DestinationRegByte = 0,
                DestinationRegBitShiftLeft = 0,
                DefaultSize = InstructionSize.DWord
            }); // register
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x6A },
                DestinationImmediate = true,
                DefaultSize = InstructionSize.DWord
            }); // immediate
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xFF },
                NeedsModRMByte = true,
                DestinationMemory = true,
                InitialModRMByteValue = 0x30,
                DefaultSize = InstructionSize.DWord
            }); // pop to memory
        }

        public Push() {
            //Changed without size
            //Size = 32;
        }
        public override string ToString() {
            return this.mMnemonic + " dword " + this.GetDestinationAsString();
        }


    }
}
