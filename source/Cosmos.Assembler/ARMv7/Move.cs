namespace Cosmos.Assembler.ARMv7
{
    [OpCode("MOV")]
    public class Move : InstructionWithOptionalFlagsUpdateAndDestinationAndOperand2
    {
        public Move() : base("MOV")
        {
        }
    }
}
