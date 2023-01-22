using System;

namespace ZLibrary.Machine.Opcodes._1OP
{
    public class CALL_1S : Opcode
    {
        public CALL_1S(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP: 0x08 call_1s routine -> (result)";
        }

        public override void Execute(ushort aRoutineAddress)
        {
            throw new NotImplementedException();
        }
    }
}
