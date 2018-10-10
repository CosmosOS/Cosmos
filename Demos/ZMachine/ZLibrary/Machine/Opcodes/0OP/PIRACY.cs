using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class PIRACY : Opcode
    {
        public PIRACY(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x0F piracy? (label)";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
