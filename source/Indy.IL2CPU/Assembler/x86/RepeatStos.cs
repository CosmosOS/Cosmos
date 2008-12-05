using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("rep stos")]
	public class RepeatStos: InstructionWithSize {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xF3, 0xAA },
                OperandSizeByte=1,
                DefaultSize=InstructionSize.DWord
            });
        }

        public override string ToString() {
            switch(Size) {
                case 32:
                    return "rep stosd";
                case 16:
                    return "rep stosw";
                case 8:
                    return "rep stosb";
                    default:throw new Exception("Size not supported!");
            }
        }
	}
}
