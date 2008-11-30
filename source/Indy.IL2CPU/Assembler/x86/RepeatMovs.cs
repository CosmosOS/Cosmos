using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("rep movs")]
    public class RepeatMovs : InstructionWithSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode=new byte[] {0xF3, 0xA4},
                OperandSizeByte = 1,
                DefaultSize = InstructionSize.Word
            });
        }
        public override string ToString() {
            switch (Size) {
                case 32:
                    return "rep movsd";
                case 16:
                    return "rep movsw";
                case 8:
                    return "rep movsb";
                default: throw new Exception("Size not supported!");
            }
        }
	}
}
