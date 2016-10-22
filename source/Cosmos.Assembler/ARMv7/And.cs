namespace Cosmos.Assembler.ARMv7
{
    [OpCode("AND")]
    public class And : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public And() : base("AND")
        {
        }
    }
}
