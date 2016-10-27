namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SDIV")]
    public class SignedDivide : InstructionWithDestinationAndTwoOperands
    {
        public SignedDivide() : base("SDIV")
        {
        }
    }
}
