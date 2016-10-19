namespace Cosmos.Assembler.ARMv7
{
    [OpCode("B")]
    public class Branch : InstructionWithLabel
    {
        public Branch() : base("B")
        {
        }
    }
}
