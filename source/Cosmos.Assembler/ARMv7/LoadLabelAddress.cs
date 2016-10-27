namespace Cosmos.Assembler.ARMv7
{
    [OpCode("ADRL")]
    public class LoadLabelAddress : InstructionWithDestinationAndLabel
    {
        public LoadLabelAddress() : base("ADRL")
        {
        }
    }
}
