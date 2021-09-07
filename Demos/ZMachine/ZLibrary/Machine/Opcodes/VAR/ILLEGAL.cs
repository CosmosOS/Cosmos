namespace ZLibrary.Machine.Opcodes.VAR
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

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {

        }
    }
}
