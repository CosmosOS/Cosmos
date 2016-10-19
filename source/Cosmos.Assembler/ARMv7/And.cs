namespace Cosmos.Assembler.ARMv7
{
    [OpCode("AND")]
    public class And : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public And() : base("AND")
        {
        }
    }
}
