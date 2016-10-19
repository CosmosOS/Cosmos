namespace Cosmos.Assembler.ARMv7
{
    [OpCode("LSL")]
    public class LogicalShiftLeft : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public LogicalShiftLeft() : base("LSL")
        {
        }
    }
}
