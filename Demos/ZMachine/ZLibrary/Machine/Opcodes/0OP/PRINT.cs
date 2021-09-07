namespace ZLibrary.Machine.Opcodes._0OP
{
    /// <summary>
    /// Print a string embedded in the instruction stream.
    /// </summary>
    public class PRINT : Opcode
    {
        public PRINT(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x02 print (literal-string)";
        }

        public override void Execute()
        {
            string s = ZText.DecodeStringWithLen((ushort)Machine.Memory.PC, out int length);
            Machine.Memory.PC += length;
            Machine.Output.PrintString(s);
        }
    }
}
