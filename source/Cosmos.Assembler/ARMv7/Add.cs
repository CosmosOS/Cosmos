namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADD")]
    public class Add : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public Add() : base("ADD")
        {
        }
    }
}
