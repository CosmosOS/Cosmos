namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SEL")]
    public class SelectBytes : InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands
    {
        public SelectBytes() : base("SEL")
        {
        }
    }
}
