using ZLibrary.Constants;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class PRINT_PADDR : Opcode
    {
        public PRINT_PADDR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0D print_paddr packed-address-of-string";
        }

        public override void Execute(ushort aPackedAddress)
        {
            long byteAddress = 0;
            if (Machine.Story.Header.Version <= FileVersion.V3)
            {
                byteAddress = aPackedAddress << 1;
            }
            else if (Machine.Story.Header.Version <= FileVersion.V5)
            {
                byteAddress = aPackedAddress << 2;
            }

            string s = ZText.DecodeStringWithLen(byteAddress, out _);
            Machine.Output.PrintString(s);
        }
    }
}
