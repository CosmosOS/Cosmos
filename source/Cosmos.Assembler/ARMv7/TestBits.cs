namespace Cosmos.Assembler.ARMv7
{
    [OpCode("TST")]
    public class TestBits : InstructionWithTwoOperands
    {
        public TestBits() : base("TST")
        {
        }
    }
}
