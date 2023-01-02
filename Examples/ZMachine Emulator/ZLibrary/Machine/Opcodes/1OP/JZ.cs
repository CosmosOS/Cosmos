namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Jump if value is zero
    /// </summary>
    public class JZ : Opcode
    {
        public JZ(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x01 jz a ? (label)";
        }

        public override void Execute(ushort aValue)
        {
            Branch((short) aValue == 0);
        }
    }
}
