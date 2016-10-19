namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SBC")]
    public class SubtractWithCarry : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public SubtractWithCarry() : base("SBC")
        {
        }
    }
}
