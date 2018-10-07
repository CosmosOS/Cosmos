namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// write a byte into a table of byte.
    /// </summary>
    public class STOREB : Opcode
    {
        public STOREB(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x01 storeb array byte-index value";
        }

        public override void Execute(ushort aTableAddress, ushort aEntryIndex, ushort aValue, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Memory.SetByte((aTableAddress + aEntryIndex), (byte)aValue);
        }
    }
}
