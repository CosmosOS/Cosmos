namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithReglist
    {
        RegistersEnum[] Reglist
        {
            get;
            set;
        }
    }
}
