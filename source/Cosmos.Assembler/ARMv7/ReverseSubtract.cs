namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RSB")]
    public class ReverseSubtract : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public ReverseSubtract() : base("RSB")
        {
        }
    }
}
