namespace Cosmos.Assembler.ARMv7
{
    [OpCode("TST")]
    public class TestBits : InstructionWithOperandAndOperand2
    {
        public TestBits() : base("TST")
        {
        }
    }
}
