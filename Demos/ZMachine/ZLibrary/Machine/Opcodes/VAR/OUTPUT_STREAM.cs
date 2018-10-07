using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class OUTPUT_STREAM : Opcode
    {
        public OUTPUT_STREAM(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x13 output_stream number, output_stream number table, output_stream number table width";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
