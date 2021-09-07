using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class OR : Opcode
    {
        public OR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x08 OR a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            throw new NotImplementedException();
        }
    }
}
