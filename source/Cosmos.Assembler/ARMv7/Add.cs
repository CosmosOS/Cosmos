namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADD")]
    public class Add : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public Add() : base("ADD")
        {
        }
    }
}
