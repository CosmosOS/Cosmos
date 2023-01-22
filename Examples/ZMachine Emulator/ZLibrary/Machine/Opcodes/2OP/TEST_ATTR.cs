namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Branch if an object attribute is set.
    /// </summary>
    public class TEST_ATTR : Opcode
    {
        public TEST_ATTR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x0A TEST_ATTR object attribute ?(label)";
        }

        public override void Execute(ushort aObject, ushort aAttribute)
        {
            if (aObject == 0)
            {
                Branch(false);
                return;
            }

            ushort objAddress = (ushort)(Machine.GetObjectAddress(aObject) + aAttribute / 8);
            Machine.Memory.GetByte(objAddress, out byte value);
            Branch((value & (0x80 >> (aAttribute & 7))) > 0);
        }
    }
}
