namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ORR")]
    public class Or : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public Or() : base("ORR")
        {
        }
    }
}
