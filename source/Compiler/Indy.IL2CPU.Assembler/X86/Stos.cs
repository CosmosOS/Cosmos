using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.Assembler.X86 {
    [OpCode("stos")]
    public class Stos : InstructionWithSize, IInstructionWithPrefix {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xAA },
                OperandSizeByte = 0,
                DefaultSize = InstructionSize.DWord
            });
        }

        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public override string ToString() {
            var xPref = "";
            if ((Prefixes & InstructionPrefixes.Repeat) != 0) {
                xPref = "rep ";
            }
            switch (Size) {
                case 32:
                    return xPref + "stosd";
                case 16:
                    return xPref + "stosw";
                case 8:
                    return xPref + "stosb";
                default: throw new Exception("Size not supported!");
            }
        }
    }
}