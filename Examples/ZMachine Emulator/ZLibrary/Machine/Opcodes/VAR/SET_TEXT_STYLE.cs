using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class SET_TEXT_STYLE : Opcode
    {
        public SET_TEXT_STYLE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x11 set_text_style style";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
