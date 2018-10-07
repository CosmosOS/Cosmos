namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Read a value from a table of bytes.
    /// </summary>
    public class LOADB : Opcode
    {
        public LOADB(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x10 loadb array byte-index -> (result)";
        }

        public override void Execute(ushort aTableAddress, ushort aEntryIndex)
        {
            ushort addr = (ushort)(aTableAddress + aEntryIndex);
            byte value;

            Machine.Memory.GetByte(addr, out value);

            Store(value);
        }
    }
}
