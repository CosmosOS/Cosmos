namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RSC")]
    public class ReverseSubtractWithCarry : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public ReverseSubtractWithCarry() : base("RSC")
        {
        }
    }
}
