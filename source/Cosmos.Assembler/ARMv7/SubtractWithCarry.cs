namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SBC")]
    public class SubtractWithCarry : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public SubtractWithCarry() : base("SBC")
        {
        }
    }
}
