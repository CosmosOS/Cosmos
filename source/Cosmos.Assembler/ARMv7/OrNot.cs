namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ORN")]
    public class OrNot : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public OrNot() : base("ORN")
        {
        }
    }
}
