using System;

namespace ZLibrary.Machine.Opcodes._0OP
{
    public class CATCH : Opcode
    {
        public CATCH(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "OP:0x09 catch -> (result)";
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
