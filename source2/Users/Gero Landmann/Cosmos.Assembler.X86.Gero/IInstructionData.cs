using System;
namespace Cosmos.Assembler.X86
{
    public interface IInstructionData
    {
        InstructionSet InstructionSet { get; set; }
        BitSize Size { get; set; }

        Operand Op1 { get; set; }
        Operand Op2 { get; set; }
        Operand Op3 { get; set; }
        Operand Op4 { get; set; }

        string ToString( InstructionOutputFormat aFormat );
    }
}
