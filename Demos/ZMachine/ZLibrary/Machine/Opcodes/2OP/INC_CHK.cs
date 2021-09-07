namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Increment a variable and branch greater than value.
    /// </summary>
    public class INC_CHK : Opcode
    {
        public INC_CHK(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x05 INC_CHK (variable) value ?(label)";
        }

        public override void Execute(ushort aVariable, ushort aValue)
        {
            ushort v;

            if (aVariable == 0)
            {
                v = ++(Machine.Memory.Stack[Machine.Memory.Stack.SP]);
            }
            else if (aVariable < 16)
            {
                v = ++(Machine.Memory.Stack[Machine.Memory.Stack.BP - aVariable]);
            }
            else
            {
                ushort addr = (ushort)(Machine.Story.Header.GlobalsOffset + 2 * (aVariable - 16));
                Machine.Memory.GetWord(addr, out v);
                v++;
                Machine.Memory.SetWord(addr, v);
            }

            Branch((short)v > (short)aValue);
        }

        public override void Execute(ushort aVariable, ushort aValue, ushort aArg2, ushort aArg3, ushort aArg4,
            ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Execute(aVariable, aValue);
        }
    }
}
