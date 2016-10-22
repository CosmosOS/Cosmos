namespace Cosmos.Assembler.ARMv7
{
    [OpCode("EOR")]
    public class ExclusiveOr : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public ExclusiveOr() : base("EOR")
        {
        }
    }
}
