namespace ZLibrary.Machine.Opcodes.VAR
{
    public class AREAD : Opcode
    {
        public AREAD(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x04 aread text parse time routine -> (result)";
        }

        public override void Execute(ushort aCharBufferAddress, ushort aTokenBufferAddress, ushort aTimeoutSeconds, ushort aTimeoutCallback, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            ushort terminator = Machine.Input.Read(aCharBufferAddress, aTokenBufferAddress, aTimeoutSeconds, aTimeoutCallback);
            Store(terminator);
        }
    }
}
