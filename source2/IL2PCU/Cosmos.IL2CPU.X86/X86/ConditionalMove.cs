using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
    [OpCode("cmovcc")]
    public class ConditionalMove: InstructionWithDestinationAndSourceAndSize, IInstructionWithCondition {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] {0x0F, 0x40},
                NeedsModRMByte=true,
                InitialModRMByteValue=0xC0,
                DestinationRegAny=true,
                SourceRegAny = true,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            }); // reg to reg
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0x0F, 0x40},
                NeedsModRMByte=true,
                DestinationRegAny=true,
                SourceMemory = true,
                AllowedSizes = InstructionSizes.DWord | InstructionSizes.Word
            });

        }
        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText(Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            mMnemonic = "cmov" + Condition.GetMnemonic();
            base.WriteText(aAssembler, aOutput);
        }
    }
}