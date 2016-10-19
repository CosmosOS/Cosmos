namespace Cosmos.Assembler.ARMv7
{
    [OpCode("TEQ")]
    public class TestEquivalence : InstructionWithTwoOperands
    {
        public TestEquivalence() : base("TEQ")
        {
        }
    }
}
