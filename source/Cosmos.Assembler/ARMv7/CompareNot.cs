namespace Cosmos.Assembler.ARMv7
{
    [OpCode("CMN")]
    public class CompareNot : InstructionWithOperandAndOperand2
    {
        public CompareNot() : base("CMN")
        {
        }
    }
}
