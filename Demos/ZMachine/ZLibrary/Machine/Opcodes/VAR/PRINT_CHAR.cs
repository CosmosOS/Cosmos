namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Print a single zscii character.
    /// </summary>
    public class PRINT_CHAR : Opcode
    {
        public PRINT_CHAR(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x05 print_char output-character-code";
        }

        public override void Execute(ushort aChar, ushort aArg1, ushort aArg2, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Output.PrintZSCII((short) aChar);
        }
    }
}
