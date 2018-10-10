namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Decrement a variable and branch if less than a value.
    /// </summary>
    public class DEC_CHK : Opcode
    {
        public DEC_CHK(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x04 DEC_CHK (variable) value ?(label)";
        }

        public override void Execute(ushort aVariable, ushort aValue)
        {
            ushort v;

            if (aVariable == 0)
            {
                v = --(Machine.Memory.Stack[Machine.Memory.Stack.SP]);
            }
            else if (aVariable < 16)
            {
                v = --(Machine.Memory.Stack[Machine.Memory.Stack.BP - aVariable]);
            }
            else
            {
                ushort xAddress = (ushort)(Machine.Story.Header.GlobalsOffset + 2 * (aVariable - 16));
                Machine.Memory.GetWord(xAddress, out v);
                v--;
                Machine.Memory.SetWord(xAddress, v);
            }

            Branch((short)v < (short)aValue);
        }
    }
}
