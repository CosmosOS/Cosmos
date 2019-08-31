namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Jump if the first value is equal to the second
    /// </summary>
    public class JE : Opcode
    {
        public JE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x01 je a b ?(label)";
        }

        public override void Execute(ushort aArg0, ushort aArg1)
        {
            bool branch = aArg0 == aArg1;
            Branch(branch);
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            bool branch1 = aArgCount > 1 && (aArg0 == aArg1);
            bool branch2 = aArgCount > 2 && (aArg0 == aArg2);
            bool branch3 = aArgCount > 3 && (aArg0 == aArg3);
            bool branch = branch1 || branch2 || branch3;
            Branch(branch);
        }
    }
}
