using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class GET_NEXT_PROP : Opcode
    {
        public GET_NEXT_PROP(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x13 get_next_prop object property -> (result)";
        }

        public override void Execute(ushort aObject, ushort aProperty)
        {
            throw new NotImplementedException();
        }
    }
}
