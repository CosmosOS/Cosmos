namespace Cosmos.Assembler.ARMv7
{
    [OpCode("BIC")]
    public class BitClear : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public BitClear() : base("BIC")
        {
        }
    }
}
