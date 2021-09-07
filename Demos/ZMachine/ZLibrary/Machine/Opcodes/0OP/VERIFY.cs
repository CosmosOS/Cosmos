using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class VERIFY : Opcode
    {
        public VERIFY(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0D verify? (label)";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
