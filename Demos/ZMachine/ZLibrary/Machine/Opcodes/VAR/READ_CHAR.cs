using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class READ_CHAR : Opcode
    {
        public READ_CHAR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x16 read_char 1 time routine -> (result)";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
