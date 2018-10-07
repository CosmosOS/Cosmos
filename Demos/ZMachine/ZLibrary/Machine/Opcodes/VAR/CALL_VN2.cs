using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class CALL_VN2 : Opcode
    {
        public CALL_VN2(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x1A call_vn2 routine ...up to 7 args...";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
