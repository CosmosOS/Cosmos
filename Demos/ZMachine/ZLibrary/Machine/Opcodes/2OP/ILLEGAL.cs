namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Exit game because an unknown opcode has been hit.
    /// </summary>
    public class ILLEGAL : Opcode
    {
        public ILLEGAL(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "ILLEGAL";
        }

        public override void Execute(ushort aArg0, ushort aArg1)
        {

        }
    }
}
