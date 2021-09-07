using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class ENCODE_TEXT : Opcode
    {
        public ENCODE_TEXT(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x1C encode_text zscii-text length from coded-text";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
