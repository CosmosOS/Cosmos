namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Print an object description.
    /// </summary>
    public class PRINT_OBJ : Opcode
    {
        public PRINT_OBJ(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0A print_obj object";
        }

        public override void Execute(ushort aObject)
        {
            string s = Machine.GetObjectName(aObject);
            Machine.Output.PrintString(s);
        }
    }
}
