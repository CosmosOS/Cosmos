namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithSecondOperand
    {
        RegistersEnum? SecondOperandReg
        {
            get;
            set;
        }
    }
}
