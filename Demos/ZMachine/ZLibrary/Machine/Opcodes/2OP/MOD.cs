using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class MOD : Opcode
    {
        public MOD(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x18 mod a b -> (result)";
        }

        public override void Execute(ushort aValue1, ushort aValue2)
        {
            throw new NotImplementedException();
        }
    }
}
