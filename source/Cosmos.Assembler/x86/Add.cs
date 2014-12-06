namespace Cosmos.Assembler.x86
{
    [Cosmos.Assembler.OpCode("add")]
    public class Add : InstructionWithDestinationAndSourceAndSize
    {
        public Add()
            : base("add")
        {
        }
    }
}