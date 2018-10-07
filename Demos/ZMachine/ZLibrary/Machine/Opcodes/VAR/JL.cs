namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Branch if the first value is less than the second.
    /// </summary>
    public class JL : Opcode
    {
        public JL(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x02 JL a b ?(label)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Branch((short) aValue1 < (short) aValue2);
        }

        public override void Execute(ushort aValue1, ushort aValue2, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Branch((short)aValue1 < (short)aValue2);
        }
    }
}
