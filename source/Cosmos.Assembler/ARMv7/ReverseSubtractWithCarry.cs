namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RSC")]
    public class ReverseSubtractWithCarry : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public ReverseSubtractWithCarry() : base("RSC")
        {
        }
    }
}
