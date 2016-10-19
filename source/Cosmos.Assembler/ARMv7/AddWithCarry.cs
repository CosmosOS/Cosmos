namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADC")]
    public class AddWithCarry : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public AddWithCarry() : base("ADC")
        {
        }
    }
}
