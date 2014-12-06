namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("cmp")]
    public class Compare : InstructionWithDestinationAndSourceAndSize
    {
        public Compare()
            : base("cmp")
        {
        }
    }
}