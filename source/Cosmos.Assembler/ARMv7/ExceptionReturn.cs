namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ERET")]
    public class ExceptionReturn : Instruction
    {
        public ExceptionReturn() : base("ERET")
        {
        }
    }
}
