using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class CALL_1N : Opcode
    {
        public CALL_1N(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0F call_1n routine";
        }

        public override void Execute(ushort aRoutineAddress)
        {
            throw new NotImplementedException();
        }
    }
}
