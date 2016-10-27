namespace Cosmos.Assembler.ARMv7
{
    [OpCode("UDIV")]
    public class UnsignedDivide : InstructionWithDestinationAndTwoOperands
    {
        public UnsignedDivide() : base("UDIV")
        {
        }
    }
}
