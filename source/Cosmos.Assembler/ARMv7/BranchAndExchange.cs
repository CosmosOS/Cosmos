namespace Cosmos.Assembler.ARMv7
{
    [OpCode("BX")]
    public class BranchAndExchange : InstructionWithDestination
    {
        public BranchAndExchange() : base("BX")
        {
        }
    }
}
