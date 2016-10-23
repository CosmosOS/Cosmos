namespace Cosmos.Assembler.ARMv7
{
    [OpCode("CMP")]
    public class Compare : InstructionWithOperandAndOperand2
    {
        public Compare() : base("CMP")
        {
        }
    }
}
