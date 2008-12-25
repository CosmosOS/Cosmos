using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("cdq")]
    public class SignExtendAX : InstructionWithSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x98 },
                AllowedSizes = InstructionSizes.Word,
                DefaultSize = InstructionSize.Word
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x66, 0x98 },
                AllowedSizes = InstructionSizes.Byte,
                DefaultSize = InstructionSize.Byte
            });
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x99 },
                AllowedSizes = InstructionSizes.DWord,
                DefaultSize = InstructionSize.DWord
            });
        }

        public override string ToString() {
            switch (Size) {
                case 32:
                    return "cdq";
                case 16:
                    return "cwde";
                case 8:
                    return "cbw";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}