using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class REMOVE_OBJ : Opcode
    {
        public REMOVE_OBJ(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP: 0x09 remove_obj object";
        }

        public override void Execute(ushort aObject)
        {
            throw new NotImplementedException();
        }
    }
}
