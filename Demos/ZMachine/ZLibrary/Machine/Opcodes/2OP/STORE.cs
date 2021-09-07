namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Store a value in a variable.
    /// </summary>
    public class STORE : Opcode
    {
        public STORE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x0D store (variable) value";
        }

        public override void Execute(ushort aVariable, ushort aValue)
        {
            if (aVariable == 0)
            {
                Machine.Memory.Stack[Machine.Memory.Stack.SP] = aValue;
            }
            else if (aVariable < 16)
            {
                Machine.Memory.Stack[Machine.Memory.Stack.BP - aVariable] = aValue;
            }
            else
            {
                ushort xAddress = (ushort)(Machine.Story.Header.GlobalsOffset + 2 * (aVariable - 16));
                Machine.Memory.SetWord(xAddress, aValue);
            }
        }

        public override void Execute(ushort aVariable, ushort aValue, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Execute(aVariable, aValue);
        }
    }
}
