using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class QUIT : Opcode
    {
        public QUIT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0A quit";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
