namespace ZLibrary.Machine.Opcodes._0OP
{
    public class POP : Opcode
    {
        public POP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x09 pop";
        }

        public override void Execute()
        {
            Machine.Memory.Stack.Pop();
        }
    }
}
