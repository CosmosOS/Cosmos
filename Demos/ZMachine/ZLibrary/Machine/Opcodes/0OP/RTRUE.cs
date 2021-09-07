namespace ZLibrary.Machine.Opcodes._0OP
{
    /// <summary>
    /// Return from a routine with true (1).
    /// </summary>
    public class RTRUE : Opcode
    {
        public RTRUE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x00 rtrue";
        }

        public override void Execute()
        {
            Return(1);
        }
    }
}
