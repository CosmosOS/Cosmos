using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class CALL_2S : Opcode
    {
        public CALL_2S(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x19 call_2s routine arg1 -> (result)";
        }

        public override void Execute(ushort aRoutineAddress, ushort aArg)
        {
            throw new NotImplementedException();
        }
    }
}
