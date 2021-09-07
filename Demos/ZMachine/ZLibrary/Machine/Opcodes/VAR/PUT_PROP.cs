using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Set the value of an object property.
    /// </summary>
    public class PUT_PROP : Opcode
    {
        public PUT_PROP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x03 put_prop object property value";
        }

        public override void Execute(ushort aObject, ushort aProperty, ushort aValue, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            byte val;

            if (aObject == 0)
            {
                return;
            }

            byte mask = (byte)((Machine.Story.Header.Version <= FileVersion.V3) ? 0x1f : 0x3f);
            ushort propAddress = Machine.GetFirstProperty(aObject);

            for (; ; )
            {
                Machine.Memory.GetByte(propAddress, out val);
                if ((val & mask) <= aProperty)
                {
                    break;
                }
                propAddress = Machine.GetNextProperty(propAddress);
            }

            propAddress++;

            if ((Machine.Story.Header.Version <= FileVersion.V3 && !((val & 0xe0) > 0)) ||
                (Machine.Story.Header.Version >= FileVersion.V4 && !((val & 0xc0) > 0)))
            {
                Machine.Memory.SetByte(propAddress, (byte)aValue);
            }
            else
            {
                Machine.Memory.SetWord(propAddress, aValue);
            }
        }
    }
}
