namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithMemoryAddress
    {
        RegistersEnum? BaseMemoryAddressReg
        {
            get;
            set;
        }

        RegistersEnum? MemoryAddressOffsetReg
        {
            get;
            set;
        }

        short? MemoryAddressOffsetValue
        {
            get;
            set;
        }

        MemoryAddressOffsetType MemoryAddressOffsetType
        {
            get;
            set;
        }

        OptionalShift MemoryAddressOptionalShift
        {
            get;
            set;
        }
    }
}
