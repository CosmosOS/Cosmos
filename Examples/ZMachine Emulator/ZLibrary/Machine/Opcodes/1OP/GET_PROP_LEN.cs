using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class GET_PROP_LEN : Opcode
    {
        public GET_PROP_LEN(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x04 get_prop_len property-address -> (result)";
        }

        public override void Execute(ushort aPropertyAddress)
        {
            ushort addr;
            byte value;

            /* Back up the property pointer to the property id */

            addr = (ushort)(aPropertyAddress - 1);
            Machine.Memory.GetByte(addr, out value);

            /* Calculate length of property */

            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                value = (byte)((value >> 5) + 1);
            }
            else if (!((value & 0x80) > 0))
            {
                value = (byte)((value >> 6) + 1);
            }
            else
            {

                value &= 0x3f;

                if (value == 0)
                {
                    value = 64; /* demanded by Spec 1.0 */
                }
            }

            /* Store length of property */

            Store(value);
        }
    }
}
