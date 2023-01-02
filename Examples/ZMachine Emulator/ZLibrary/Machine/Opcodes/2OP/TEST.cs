namespace ZLibrary.Machine.Opcodes._2OP
{
    public class TEST : Opcode
    {
        public TEST(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x07 TEST bitmap flags ?(label)";
        }

        public override void Execute(ushort aBitmap, ushort aFlags)
        {
            Branch((aBitmap & aFlags) == aFlags);
        }
    }
}
