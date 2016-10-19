namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithDestination
    {
        RegistersEnum? DestinationReg
        {
            get;
            set;
        }
    }
}
