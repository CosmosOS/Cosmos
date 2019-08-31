namespace ZLibrary.Machine.Opcodes._0OP
{
    /// <summary>
    /// Return from a routine with a value popped off the stack.
    /// </summary>
    public class RET_POPPED : Opcode
    {
        public RET_POPPED(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x08 ret_popped";
        }

        public override void Execute()
        {
            ushort xValue = Machine.Memory.Stack.Pop();
            Return(xValue);
        }
    }
}
