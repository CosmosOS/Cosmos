namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SUB")]
    public class Subtract : InstructionWithOptionalSuffixAndDestinationAndTwoOperands
    {
        public Subtract() : base("SUB")
        {
        }
    }
}
