namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// 16-bit SUB
    /// </summary>
    public class SUB : Opcode
    {
        public SUB(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x15 sub a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Store((ushort) ((short) aValue1 - (short) aValue2));
        }
    }
}
