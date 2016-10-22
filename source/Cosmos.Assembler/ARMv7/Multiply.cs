namespace Cosmos.Assembler.ARMv7
{
    [OpCode("MUL")]
    public class Multiply : InstructionWithOptionalFlagsUpdateAndDestinationAndTwoOperands
    {
        public Multiply() : base("MUL")
        {
        }
    }
}
