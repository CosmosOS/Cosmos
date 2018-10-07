namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Jump unconditionally to the given address.
    /// </summary>
    public class JUMP : Opcode
    {
        public JUMP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0C jump ?(label)";
        }

        public override void Execute(ushort aAddress)
        {
            Machine.Memory.PC += (short)aAddress - 2;
        }
    }
}
