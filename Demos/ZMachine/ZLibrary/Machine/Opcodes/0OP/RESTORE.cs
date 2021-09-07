using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class RESTORE : Opcode
    {
        public RESTORE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x06 restore? (label), restore -> (result)";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
