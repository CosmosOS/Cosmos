using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class CALL_2N : Opcode
    {
        public CALL_2N(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x1A call_2s routine arg1";
        }

        public override void Execute(ushort aRoutineAddress, ushort aArg)
        {
            throw new NotImplementedException();
        }
    }
}
