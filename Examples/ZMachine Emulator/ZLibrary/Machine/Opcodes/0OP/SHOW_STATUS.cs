using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class SHOW_STATUS : Opcode
    {
        public SHOW_STATUS(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0C show_status";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
