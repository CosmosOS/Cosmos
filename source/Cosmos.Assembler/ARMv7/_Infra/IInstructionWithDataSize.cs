namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithDataSize
    {
        DataSize? DataSize
        {
            get;
            set;
        }
    }
}
