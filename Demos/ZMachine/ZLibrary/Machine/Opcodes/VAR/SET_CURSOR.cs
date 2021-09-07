using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class SET_CURSOR : Opcode
    {
        public SET_CURSOR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x0F set_cursor line column, set_cursor line column window";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
