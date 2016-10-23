namespace Cosmos.Assembler.ARMv7
{
    [OpCode("WFE")]
    public class WaitForEvent : Instruction
    {
        public WaitForEvent() : base("WFE")
        {
        }
    }
}
