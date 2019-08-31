using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class SOUND_EFFECT : Opcode
    {
        public SOUND_EFFECT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x15 sound_effect number effect volume routine";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
