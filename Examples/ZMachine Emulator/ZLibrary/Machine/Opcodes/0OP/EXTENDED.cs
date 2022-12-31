using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class EXTENDED : Opcode
    {
        public EXTENDED(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0E [first byte of extended opcode]";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
