using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Branch if the first object is inside the second.
    /// </summary>
    public class JIN : Opcode
    {
        public JIN(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x06 JIN obj1 obj2 ?(label)";
        }

        public override void Execute(ushort aObject1, ushort aObject2)
        {
            if (aObject1 == 0)
            {
                Branch(0 == aObject2);
                return;
            }

            ushort objAddress = Machine.GetObjectAddress(aObject1);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                objAddress += ZObject.O1_PARENT;
                Machine.Memory.GetByte(objAddress, out byte parent);
                Branch(parent == aObject2);
            }
            else
            {
                objAddress += ZObject.O4_PARENT;
                Machine.Memory.GetWord(objAddress, out ushort parent);
                Branch(parent == aObject2);
            }
        }
    }
}
