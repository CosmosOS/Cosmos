using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class CHECK_ARG_COUNT : Opcode
    {
        public CHECK_ARG_COUNT(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x1F check_arg_count argument-number";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
