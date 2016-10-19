namespace Cosmos.Assembler.ARMv7
{
    [OpCode("BL")]
    public class BranchWithLink : InstructionWithLabel
    {
        public BranchWithLink() : base("BL")
        {
        }
    }
}
