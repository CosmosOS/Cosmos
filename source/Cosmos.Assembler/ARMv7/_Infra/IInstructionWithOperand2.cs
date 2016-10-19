namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOperand2
    {
        RegistersEnum? SecondOperandReg
        {
            get;
            set;
        }

        uint SecondOperandValue
        {
            get;
            set;
        }
    }
}
