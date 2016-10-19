namespace Cosmos.Assembler.ARMv7
{
    [OpCode("BIC")]
    public class BitClear : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public BitClear() : base("BIC")
        {
        }
    }
}
