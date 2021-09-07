using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class TOKENISE : Opcode
    {
        public TOKENISE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x1B tokenise text parse dictionary flag";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
