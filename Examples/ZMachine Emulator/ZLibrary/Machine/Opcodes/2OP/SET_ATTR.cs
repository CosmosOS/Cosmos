namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Set an object attribute
    /// </summary>
    public class SET_ATTR : Opcode
    {
        public SET_ATTR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x0B SET_ATTR object attribute";
        }

        public override void Execute(ushort aObject, ushort aAttribute)
        {
            if (aObject == 0)
            {
                return;
            }

            ushort objAddress = (ushort)(Machine.GetObjectAddress(aObject) + aAttribute / 8);
            Machine.Memory.GetByte(objAddress, out byte value);
            value |= (byte)(0x80 >> (aAttribute & 7));
            Machine.Memory.SetByte(objAddress, value);
        }
    }
}
