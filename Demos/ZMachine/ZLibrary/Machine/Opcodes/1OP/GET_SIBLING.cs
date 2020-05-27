using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._1OP
{
    /// <summary>
    /// Store the sibling of an object.
    /// </summary>
    public class GET_SIBLING : Opcode
    {
        public GET_SIBLING(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x01 get_sibling object -> (result)? (label)";
        }

        public override void Execute(ushort aObject)
        {
            ushort obj_addr;

            if (aObject == 0)
            {
                Store(0);
                Branch(false);
                return;
            }

            obj_addr = Machine.GetObjectAddress(aObject);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {

                byte sibling;

                obj_addr += ZObject.O1_SIBLING;
                Machine.Memory.GetByte(obj_addr, out sibling);

                Store(sibling);
                Branch(sibling > 0);
            }
            else
            {

                ushort sibling;

                obj_addr += ZObject.O4_SIBLING;
                Machine.Memory.GetWord(obj_addr, out sibling);

                Store(sibling);
                Branch(sibling > 0);
            }
        }
    }
}
