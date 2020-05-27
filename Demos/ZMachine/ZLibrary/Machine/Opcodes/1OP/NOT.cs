using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class NOT : Opcode
    {
        public NOT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0F not value -> (result)";
        }

        public override void Execute(ushort aValue)
        {
            throw new NotImplementedException();
        }
    }
}
