namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOptionalSuffix
    {
        bool UpdateFlags
        {
            get;
            set;
        }
    }
}
