using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class NOP : Opcode
    {
        public NOP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x04 nop";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
