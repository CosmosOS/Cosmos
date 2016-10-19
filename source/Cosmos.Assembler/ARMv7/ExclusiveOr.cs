namespace Cosmos.Assembler.ARMv7
{
    [OpCode("EOR")]
    public class ExclusiveOr : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public ExclusiveOr() : base("EOR")
        {
        }
    }
}
