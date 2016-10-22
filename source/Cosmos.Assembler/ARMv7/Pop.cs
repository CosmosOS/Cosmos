namespace Cosmos.Assembler.ARMv7
{
    [OpCode("POP")]
    public class Pop : InstructionWithReglist
    {
        public Pop() : base("POP")
        {
        }
    }
}
