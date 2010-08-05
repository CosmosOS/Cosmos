using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86 {
    [OpCode("movs")]
    public class Movs : InstructionWithSize, IInstructionWithPrefix {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
            aData.EncodingOptions.Add(new InstructionData.InstructionEncodingOption {
                OpCode = new byte[] { 0xA4 },
                OperandSizeByte = 0,                
                DefaultSize = InstructionSize.Byte
            });
        }
        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public override void WriteText( Cosmos.Compiler.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            if ((Prefixes & InstructionPrefixes.Repeat) != 0) {
                aOutput.Write("rep ");
            }
            switch (Size) {
                case 32:
                    aOutput.Write("movsd");
                    return;
                case 16:
                    aOutput.Write("movsw");
                    return;
                case 8:
                    aOutput.Write("movsb");
                    return;
                default: throw new Exception("Size not supported!");
            }
        }
    }
}