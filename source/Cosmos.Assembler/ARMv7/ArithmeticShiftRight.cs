namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ASR")]
    public class ArithmeticShiftRight : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public ArithmeticShiftRight() : base("ASR")
        {
        }
    }
}
