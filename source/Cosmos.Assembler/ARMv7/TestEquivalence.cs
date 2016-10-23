namespace Cosmos.Assembler.ARMv7
{
    [OpCode("TEQ")]
    public class TestEquivalence : InstructionWithOperandAndOperand2
    {
        public TestEquivalence() : base("TEQ")
        {
        }
    }
}
