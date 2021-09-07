using System;

namespace ZLibrary.Machine.Opcodes._2OP
{
    public class SET_COLOR : Opcode
    {
        public SET_COLOR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "2OP:0x1B set_colour foreground background";
        }

        public override void Execute(ushort aForeground, ushort aBackground)
        {
            throw new NotImplementedException();
        }
    }
}
