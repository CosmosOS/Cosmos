namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOperand
    {
        RegistersEnum? OperandReg
        {
            get;
            set;
        }
    }
}
