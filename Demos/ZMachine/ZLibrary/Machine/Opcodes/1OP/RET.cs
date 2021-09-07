namespace ZLibrary.Machine.Opcodes._1OP
{
    public class RET : Opcode
    {
        public RET(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "1OP:0x0B ret value";
        }

        public override void Execute(ushort aValue)
        {
            Return(aValue);
        }
    }
}
