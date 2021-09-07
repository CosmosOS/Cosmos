using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Store the parent of an object.
    /// </summary>
    public class GET_PARENT : Opcode
    {
        public GET_PARENT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x03 get_parent object -> (result)";
        }

        public override void Execute(ushort aObject)
        {
            if (aObject == 0)
            {
                Store(0);
                return;
            }

            ushort objAddress = Machine.GetObjectAddress(aObject);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                objAddress += ZObject.O1_PARENT;
                Machine.Memory.GetByte(objAddress, out byte parent);
                Store(parent);
            }
            else
            {
                objAddress += ZObject.O4_PARENT;
                Machine.Memory.GetWord(objAddress, out ushort parent);
                Store(parent);
            }
        }
    }
}
