namespace Cosmos.Assembler.x86
{
    public abstract class InstructionWithSize : Instruction, IInstructionWithSize
    {
        protected InstructionWithSize()
        {
        }

        public byte Size
        {
            get;
            set;
        }
    }
}