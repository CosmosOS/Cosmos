namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithLabel
    {
        string Label
        {
            get;
            set;
        }

        uint? LabelOffset
        {
            get;
            set;
        }
    }
}
