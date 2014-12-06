namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("push")]
    public class Push : InstructionWithDestinationAndSize
    {
        public Push()
            : base("push")
        {
            Size = 32;
        }
    }
}