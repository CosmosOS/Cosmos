using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class ERASE_LINE : Opcode
    {
        public ERASE_LINE(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x0E erase_line value, erase_line pixels";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
