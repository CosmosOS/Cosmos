namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RRX")]
    public class RotateRightWithExtend : InstructionWithOptionalSuffixAndDestinationAndOperand
    {
        public RotateRightWithExtend() : base("RRX")
        {
        }
    }
}
