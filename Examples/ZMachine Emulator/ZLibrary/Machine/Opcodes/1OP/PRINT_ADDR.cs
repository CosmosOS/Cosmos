using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class PRINT_ADDR : Opcode
    {
        public PRINT_ADDR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP: 0x07 print_addr byte-address-of-string";
        }

        public override void Execute(ushort aAdress)
        {
            throw new NotImplementedException();
        }
    }
}
