using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class CALL_VS2 : Opcode
    {
        public CALL_VS2(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x0C call_vs2 routine...0 to 7 args... -> (result)";
        }

        public override void Execute(ushort aArg0, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
