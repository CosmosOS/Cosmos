using System;

namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("movs")]
    public class Movs : InstructionWithSize, IInstructionWithPrefix
    {
        public InstructionPrefixes Prefixes { get; set; }

        public override void WriteText(Cosmos.Assembler.Assembler aAssembler, System.IO.TextWriter aOutput)
        {
            if ((Prefixes & InstructionPrefixes.Repeat) != 0)
            {
                aOutput.Write("rep ");
            }
            switch (Size)
            {
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