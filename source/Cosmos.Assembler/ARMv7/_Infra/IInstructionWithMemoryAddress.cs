namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithMemoryAddress
    {
        RegistersEnum? BaseMemoryAddressReg
        {
            get;
            set;
        }

        short? MemoryAddressOffset
        {
            get;
            set;
        }

        MemoryAddressOffsetType MemoryAddressOffsetType
        {
            get;
            set;
        }
    }
}
