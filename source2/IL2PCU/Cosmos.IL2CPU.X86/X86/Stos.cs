using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.IL2CPU.X86 {
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

        public override void WriteText( Cosmos.IL2CPU.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            if ((Prefixes & InstructionPrefixes.Repeat) != 0) {
                aOutput.Write("rep ");
            }
            switch (Size) {
                case 32:
                    aOutput.Write("stosd");
                    return;
                case 16:
                    aOutput.Write("stosw");
                    return;
                case 8:
                    aOutput.Write("stosb");
                    return;
                default: throw new Exception("Size not supported!");
            }
        }
    }
}