using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class INPUT_STREAM : Opcode
    {
        public INPUT_STREAM(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x14 input_stream number";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
