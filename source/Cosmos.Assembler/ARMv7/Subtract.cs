namespace Cosmos.Assembler.ARMv7
{
    [OpCode("SUB")]
    public class Subtract : InstructionWithOptionalFlagsUpdateAndDestinationAndOperandAndOperand2
    {
        public Subtract() : base("SUB")
        {
        }
    }
}
