namespace ZLibrary.Machine.Opcodes.VAR
{
    public class JG : Opcode
    {
        public JG(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x03 JG a b ?(label)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Branch((short) aValue1 > (short) aValue2);
        }

        public override void Execute(ushort aValue1, ushort aValue2, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Branch((short)aValue1 > (short)aValue2);
        }
    }
}
