namespace ZLibrary.Machine.Opcodes.VAR
{
    public class PRINT_NUM : Opcode
    {
        public PRINT_NUM(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x06 print_num value";
        }

        public override void Execute(ushort aValue, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Output.PrintZSCII((short) aValue);
        }
    }
}
