namespace Cosmos.Assembler.ARMv7
{
    [OpCode("CMN")]
    public class CompareNegative : InstructionWithOperandAndOperand2
    {
        public CompareNegative() : base("CMN")
        {
        }
    }
}
