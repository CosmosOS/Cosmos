using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class THROW : Opcode
    {
        public THROW(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:1C throw value stack-frame";
        }

        public override void Execute(ushort aValue, ushort aStackFrame)
        {
            throw new NotImplementedException();
        }
    }
}
