namespace ZLibrary.Machine.Opcodes._2OP
{
    public class MUL : Opcode
    {
        public MUL(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x16 mul a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Store((ushort) (aValue1 * aValue2));
        }
    }
}
