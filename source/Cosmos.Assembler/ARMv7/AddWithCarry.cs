namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADC")]
    public class AddWithCarry : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public AddWithCarry() : base("ADC")
        {
        }
    }
}
