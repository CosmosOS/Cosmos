namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ROR")]
    public class RotateRight : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public RotateRight() : base("ROR")
        {
        }
    }
}
