using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class RESTART : Opcode
    {
        public RESTART(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x07 restart";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
