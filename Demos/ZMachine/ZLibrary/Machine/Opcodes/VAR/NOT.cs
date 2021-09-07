using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class NOT : Opcode
    {
        public NOT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x18 not value -> (result)";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
