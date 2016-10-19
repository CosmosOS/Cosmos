namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ORN")]
    public class OrNot : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public OrNot() : base("ORN")
        {
        }
    }
}
