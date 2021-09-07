namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// 16-bit ADD
    /// </summary>
    public class ADD : Opcode
    {
        public ADD(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x14 add a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Store((ushort) ((short) aValue1 + (short) aValue2));
        }
    }
}
