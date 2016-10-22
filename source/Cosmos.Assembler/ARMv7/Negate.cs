#warning TODO: Should pseudo-instructions be included in Assembler?

namespace Cosmos.Assembler.ARMv7
{
    [OpCode("NEG")]
    public class Negate : InstructionWithDestinationAndOperand
    {
        public Negate() : base("NEG")
        {
        }
    }
}
