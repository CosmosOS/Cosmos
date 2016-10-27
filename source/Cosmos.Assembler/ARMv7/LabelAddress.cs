namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADR")]
    public class LabelAddress : InstructionWithDestinationAndLabel
    {
        public LabelAddress() : base("ADR")
        {
        }
    }
}
