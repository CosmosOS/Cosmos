namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("mov")]
    public class Mov : InstructionWithDestinationAndSourceAndSize
    {
        public Mov()
            : base("mov")
        {
        }
    }
}