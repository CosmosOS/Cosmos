namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Read a value from a table of words.
    /// </summary>
    public class LOADW : Opcode
    {
        public LOADW(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x0F loadw array word-index -> (result)";
        }

        public override void Execute(ushort aTableAddress, ushort aEntryIndex)
        {
            ushort addr = (ushort)(aTableAddress + 2 * aEntryIndex);
            ushort value;

            Machine.Memory.GetWord(addr, out value);

            Store(value);
        }
    }
}
