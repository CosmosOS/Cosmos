using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("cmovcc")]
    public class ConditionalMove: InstructionWithDestinationAndSourceAndSize, IInstructionWithCondition {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] {0x0F, 0x40},
                NeedsModRMByte=true,
                InitialModRMByteValue=0xC0,
                DestinationReg=Guid.Empty,
                SourceReg=Guid.Empty,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x0F, 0x40},
                NeedsModRMByte=true,
                DestinationReg=Guid.Empty,
                SourceMemory = true,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            });

        }
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override string ToString() {
            mMnemonic = "cmov" + Condition.GetMnemonic();
            return base.ToString();
        }
    }
}