namespace Cosmos.Assembler.ARMv7
{
    [OpCode("WFI")]
    public class WaitForInterrupt : Instruction
    {
        public WaitForInterrupt() : base("WFI")
        {
        }
    }
}
