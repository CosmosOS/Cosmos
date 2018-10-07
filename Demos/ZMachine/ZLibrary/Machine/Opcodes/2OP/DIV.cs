namespace ZLibrary.Machine.Opcodes._2OP
{
    public class DIV : Opcode
    {
        public DIV(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x17 div a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Store((ushort) ((short) aValue1 / (short) aValue2));
        }
    }
}
