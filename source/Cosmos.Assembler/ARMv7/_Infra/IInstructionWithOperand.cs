namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOperand
    {
        RegistersEnum? FirstOperandReg
        {
            get;
            set;
        }
    }
}
