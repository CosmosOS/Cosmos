namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ORR")]
    public class Or : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public Or() : base("ORR")
        {
        }
    }
}
