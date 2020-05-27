namespace ZLibrary.Machine.Opcodes._0OP
{
    public class PRINT_RET : Opcode
    {
        public PRINT_RET(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "0OP:0x03 print_ret (literal-string)";
        }

        public override void Execute()
        {
            string s = ZText.DecodeStringWithLen((ushort)Machine.Memory.PC, out int length);
            Machine.Memory.PC += length;
            Machine.Output.PrintString(s);
            Machine.Output.PrintString("\n");
            Return(1);
        }
    }
}
