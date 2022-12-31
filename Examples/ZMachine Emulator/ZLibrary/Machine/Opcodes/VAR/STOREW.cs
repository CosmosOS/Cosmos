namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// write a word into a table of words.
    /// </summary>
    public class STOREW : Opcode
    {
        public STOREW(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x01 storew array word-index value";
        }

        public override void Execute(ushort aTableAddress, ushort aEntryIndex, ushort aValue, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Memory.SetByte((aTableAddress + (2 * aEntryIndex)), (byte)(aValue >> 8));
            Machine.Memory.SetByte((aTableAddress + (2 * aEntryIndex) + 1), (byte)(aValue & 0xff));
        }
    }
}
