namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOptionalFlagsUpdate
    {
        bool UpdateFlags
        {
            get;
            set;
        }
    }
}
