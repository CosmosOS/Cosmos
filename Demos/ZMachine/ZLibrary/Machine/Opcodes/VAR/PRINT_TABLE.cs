using System;

namespace ZLibrary.Machine.Opcodes.VAR
{
    public class PRINT_TABLE : Opcode
    {
        public PRINT_TABLE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x1E print_table zscii-text width height skip";
        }

        public override void Execute(ushort aText, ushort aWidth, ushort aHeight, ushort aSkip, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            throw new NotImplementedException();
        }
    }
}
