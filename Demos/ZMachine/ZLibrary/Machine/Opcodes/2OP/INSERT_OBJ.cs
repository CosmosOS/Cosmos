using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Make an object the first child of another object.
    /// </summary>
    public class INSERT_OBJ : Opcode
    {
        public INSERT_OBJ(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x0E insert_obj object destination";
        }

        public override void Execute(ushort aSourceObject, ushort aDestinationObject)
        {
            ushort obj1_addr;
            ushort obj2_addr;

            if (aSourceObject == 0)
            {
                return;
            }

            if (aDestinationObject == 0)
            {
                return;
            }

            obj1_addr = Machine.GetObjectAddress(aSourceObject);
            obj2_addr = Machine.GetObjectAddress(aDestinationObject);

            Machine.UnlinkObject(aSourceObject);

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {

                byte child;

                obj1_addr += ZObject.O1_PARENT;
                Machine.Memory.SetByte(obj1_addr, (byte)aDestinationObject);
                obj2_addr += ZObject.O1_CHILD;
                Machine.Memory.GetByte(obj2_addr, out child);
                Machine.Memory.SetByte(obj2_addr, (byte)aSourceObject);
                obj1_addr += ZObject.O1_SIBLING - ZObject.O1_PARENT;
                Machine.Memory.SetByte(obj1_addr, child);

            }
            else
            {

                ushort child;

                obj1_addr += ZObject.O4_PARENT;
                Machine.Memory.SetWord(obj1_addr, aDestinationObject);
                obj2_addr += ZObject.O4_CHILD;
                Machine.Memory.GetWord(obj2_addr, out child);
                Machine.Memory.SetWord(obj2_addr, aSourceObject);
                obj1_addr += ZObject.O4_SIBLING - ZObject.O4_PARENT;
                Machine.Memory.SetWord(obj1_addr, child);

            }
        }
    }
}
