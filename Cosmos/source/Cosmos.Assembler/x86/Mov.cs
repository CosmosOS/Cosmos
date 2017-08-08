namespace Cosmos.Assembler.x86
{
    [OpCode("mov")]
    public class Mov : InstructionWithDestinationAndSourceAndSize
    {
        public Mov() : base("mov")
        {
        }
    }
}
