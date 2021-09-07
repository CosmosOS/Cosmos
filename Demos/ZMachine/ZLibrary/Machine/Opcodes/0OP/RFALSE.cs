namespace ZLibrary.Machine.Opcodes._0OP
{
    /// <summary>
    /// Return from a routine with false (0).
    /// </summary>
    public class RFALSE : Opcode
    {
        public RFALSE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x01 rfalse";
        }

        public override void Execute()
        {
            Return(0);
        }
    }
}
