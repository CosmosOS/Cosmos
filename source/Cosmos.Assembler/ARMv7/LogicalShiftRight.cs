namespace Cosmos.Assembler.ARMv7
{
    [OpCode("LSR")]
    public class LogicalShiftRight : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public LogicalShiftRight() : base("LSR")
        {
        }
    }
}
