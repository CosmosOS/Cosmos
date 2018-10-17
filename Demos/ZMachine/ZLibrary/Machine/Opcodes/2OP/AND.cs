namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Bitwise AND
    /// </summary>
    public class AND : Opcode
    {
        public AND(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x09 AND a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            Store((ushort) (aValue1 & aValue2));
        }

        public override void Execute(ushort aValue1, ushort aValue2, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Store((ushort)(aValue1 & aValue2));
        }
    }
}
