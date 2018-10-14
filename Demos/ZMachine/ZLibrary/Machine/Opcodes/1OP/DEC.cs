namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Decrement a variable.
    /// </summary>
    public class DEC : Opcode
    {
        public DEC(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP: 0x06 dec(variable)";
        }

        public override void Execute(ushort aVariable)
        {
            ushort value;

            if (aVariable == 0)
            {
                (Machine.Memory.Stack[Machine.Memory.Stack.SP])--;
            }
            else if (aVariable < 16)
            {
                (Machine.Memory.Stack[Machine.Memory.Stack.BP - aVariable])--;
            }
            else
            {
                ushort addr = (ushort) (Machine.Story.Header.GlobalsOffset + 2 * (aVariable - 16));
                Machine.Memory.GetWord(addr, out value);
                value--;
                Machine.Memory.SetWord(addr, value);
            }
        }
    }
}
