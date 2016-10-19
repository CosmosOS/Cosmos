namespace Cosmos.Assembler.ARMv7
{
    [OpCode("BLX")]
    public class BranchWithLinkAndExchange : InstructionWithLabelOrDestination
    {
        public BranchWithLinkAndExchange() : base("BLX")
        {
        }
    }
}
