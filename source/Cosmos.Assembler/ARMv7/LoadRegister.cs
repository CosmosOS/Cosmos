namespace Cosmos.Assembler.ARMv7
{
    [OpCode("LDR")]
    public class LoadRegister : InstructionWithDataSizeAndOperandsAndMemoryAddress
    {
        public LoadRegister() : base("LDR")
        {
        }
    }
}
