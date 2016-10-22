namespace Cosmos.Assembler.ARMv7
{
    [OpCode("PUSH")]
    public class Push : InstructionWithReglist
    {
        public Push() : base("PUSH")
        {
        }
    }
}
