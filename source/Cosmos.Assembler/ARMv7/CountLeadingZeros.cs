namespace Cosmos.Assembler.ARMv7
{
    [OpCode("CLZ")]
    public class CountLeadingZeros : InstructionWithDestinationAndOperand
    {
        public CountLeadingZeros() : base("CLZ")
        {
        }
    }
}
