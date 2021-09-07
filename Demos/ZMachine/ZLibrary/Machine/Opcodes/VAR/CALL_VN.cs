using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class CALL_VN : Opcode
    {
        public CALL_VN(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x19 call_vn routine ...up to 3 args...";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
