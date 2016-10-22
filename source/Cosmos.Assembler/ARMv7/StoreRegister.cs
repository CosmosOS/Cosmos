namespace Cosmos.Assembler.ARMv7
{
    [OpCode("STR")]
    public class StoreRegister : InstructionWithDataSizeAndOperandsAndMemoryAddress
    {
        public StoreRegister() : base("STR")
        {
        }
    }
}
