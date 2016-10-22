namespace Cosmos.Assembler.ARMv7
{
    [OpCode("RSB")]
    public class ReverseSubtract : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public ReverseSubtract() : base("RSB")
        {
        }
    }
}
