namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ROR")]
    public class RotateRight : ShiftInstruction
    {
        public RotateRight() : base("ROR")
        {
        }
    }
}
