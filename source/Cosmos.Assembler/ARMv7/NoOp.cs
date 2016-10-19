namespace Cosmos.Assembler.ARMv7
{
    [OpCode("NOP")]
    public class NoOp : Instruction
    {
        public NoOp() : base("NOP")
        {
        }
    }
}
