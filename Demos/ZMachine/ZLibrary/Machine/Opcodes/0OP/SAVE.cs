using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class SAVE : Opcode
    {
        public SAVE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x05 save? (label), save -> (result)";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
