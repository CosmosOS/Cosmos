namespace Cosmos.Assembler.ARMv7
{
    public interface IInstructionWithOperand2
    {
        RegistersEnum? Operand2Reg
        {
            get;
            set;
        }

        uint? Operand2Value
        {
            get;
            set;
        }

        OptionalShift Operand2Shift
        {
            get;
            set;
        }
    }
}
