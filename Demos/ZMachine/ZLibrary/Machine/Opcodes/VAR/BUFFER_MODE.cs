using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class BUFFER_MODE : Opcode
    {
        public BUFFER_MODE(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x12 buffer_mode flag";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
