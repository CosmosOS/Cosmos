using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("jcc")]
    public class ConditionalJump: JumpBase, IInstructionWithCondition {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0x0F, 0x80 },
                DestinationImmediate = true
            });

        }

        public ConditionalTestEnum Condition {
            get;
            set;
        }

        public override void WriteText(Indy.IL2CPU.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            mMnemonic = String.Intern("j" + Condition.GetMnemonic());
            base.WriteText(aAssembler, aOutput);
        }
    }
}