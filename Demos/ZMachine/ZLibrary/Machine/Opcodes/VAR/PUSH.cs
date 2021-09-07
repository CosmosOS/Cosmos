namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Push a value onto game the stack.
    /// </summary>
    public class PUSH : Opcode
    {
        public PUSH(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x08 push value";
        }

        public override void Execute(ushort aValue, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Memory.Stack.Push(aValue);
        }
    }
}
