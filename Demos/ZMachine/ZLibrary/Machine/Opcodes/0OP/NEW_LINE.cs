namespace ZLibrary.Machine.Opcodes._0OP
{
    /// <summary>
    /// Print a new line.
    /// </summary>
    public class NEW_LINE : Opcode
    {
        public NEW_LINE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0B new_line";
        }

        public override void Execute()
        {
            Machine.Output.PrintString("\n");
        }
    }
}
