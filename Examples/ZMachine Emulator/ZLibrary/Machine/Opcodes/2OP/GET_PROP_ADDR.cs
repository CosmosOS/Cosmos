using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class GET_PROP_ADDR : Opcode
    {
        public GET_PROP_ADDR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x12 get_prop_addr object property -> (result)";
        }

        public override void Execute(ushort aObject, ushort aProperty)
        {
            byte value;
            byte mask;

            if (aObject == 0)
            {
                Store(0);
                return;
            }

            mask = (byte)((Machine.Story.Header.Version <= FileVersion.V3) ? 0x1f : 0x3f);

            ushort propAddress = Machine.GetFirstProperty(aObject);

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
                if (Machine.Story.Header.Version >= FileVersion.V4 && (value & 0x80) > 0)
                {
                    propAddress++;
                }

                Store((ushort)(propAddress + 1));
            }
            else
            {
                Store(0);
            }
        }
    }
}
