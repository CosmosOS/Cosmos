namespace Cosmos.Assembler.ARMv7
{
    [OpCode("MVN")]
    public class MoveNot : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2
    {
        public MoveNot() : base("MVN")
        {
        }
    }
}
