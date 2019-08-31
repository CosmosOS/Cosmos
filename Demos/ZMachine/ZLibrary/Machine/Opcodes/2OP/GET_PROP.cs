using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._2OP
{
    /// <summary>
    /// Store the value of an object property.
    /// </summary>
    public class GET_PROP : Opcode
    {
        public GET_PROP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x11 get_prop object property -> (result)";
        }

        public override void Execute(ushort aObject, ushort aProperty)
        {
            ushort propAddress;
            ushort wprop_val;
            byte bprop_val;
            byte value;
            byte mask;

            if (aObject == 0)
            {
                Store(0);
                return;
            }

            mask = (byte)((Machine.Story.Header.Version <= FileVersion.V3) ? 0x1f : 0x3f);

            propAddress = Machine.GetFirstProperty(aObject);

            for (; ; )
            {
                Machine.Memory.GetByte(propAddress, out value);
                if ((value & mask) <= aProperty)
                {
                    break;
                }
                propAddress = Machine.GetNextProperty(propAddress);
            }

            if ((value & mask) == aProperty)
            {
                propAddress++;

                if ((Machine.Story.Header.Version <= FileVersion.V3 && !((value & 0xe0) > 0)) ||
                    (Machine.Story.Header.Version >= FileVersion.V4 && !((value & 0xc0) > 0)))
                {

                    Machine.Memory.GetByte(propAddress, out bprop_val);
                    wprop_val = bprop_val;

                }
                else
                {
                    Machine.Memory.GetWord(propAddress, out wprop_val);
                }
            }
            else
            {
                propAddress = (ushort)(Machine.Story.Header.ObjectsOffset + 2 * (aProperty - 1));
                Machine.Memory.GetWord(propAddress, out wprop_val);
            }

            Store(wprop_val);
        }
    }
}
