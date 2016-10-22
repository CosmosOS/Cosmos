namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RRX")]
    public class RotateRightWithExtend : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand
    {
        public RotateRightWithExtend() : base("RRX")
        {
        }
    }
}
