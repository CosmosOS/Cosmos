using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Compiler.Assembler.X86.X86
{
    [OpCode("cmps")]
    public class Cmps: InstructionWithSize, IInstructionWithPrefix {
        public static void InitializeEncodingData(Instruction.InstructionData aData) {
        }

        public InstructionPrefixes Prefixes {
            get;
            set;
        }

        public override void WriteText(Cosmos.Compiler.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput )
        {
            if ((Prefixes & InstructionPrefixes.RepeatTillEqual) != 0)
            {
                aOutput.Write("repne ");
            }
            switch (Size) {
                case 32:
                    aOutput.Write("cmpsd");
                    return;
                case 16:
                    aOutput.Write("cmpsw");
                    return;
                case 8:
                    aOutput.Write("smpsb");
                    return;
                default: throw new Exception("Size not supported!");
            }
        }
    }
}
