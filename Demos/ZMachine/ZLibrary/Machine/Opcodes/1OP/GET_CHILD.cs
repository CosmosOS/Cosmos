using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Store the child of an object.
    /// </summary>
    public class GET_CHILD : Opcode
    {
        public GET_CHILD(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x02 get_child object -> (result) ? (label)";
        }

        public override void Execute(ushort aObject)
        {
            if (aObject == 0)
            {
                Store(0);
                Branch(false);
                return;
            }

            ushort objAddress = Machine.GetObjectAddress(aObject);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                objAddress += ZObject.O1_CHILD;
                Machine.Memory.GetByte(objAddress, out byte child);
                Store(child);
                Branch(child > 0);

            }
            else
            {
                objAddress += ZObject.O4_CHILD;
                Machine.Memory.GetWord(objAddress, out ushort child);
                Store(child);
                Branch(child > 0);
            }
        }
    }
}
