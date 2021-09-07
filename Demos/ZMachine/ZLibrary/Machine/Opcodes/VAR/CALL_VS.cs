using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class CALL_VS : Opcode
    {
        public CALL_VS(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x00 call_vs routine [arg1, arg2, arg3] -> (result) call";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
