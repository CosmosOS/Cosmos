namespace ZLibrary.Machine.Opcodes.VAR
{

    public class SREAD : Opcode
    {
        public SREAD(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x04 sread text parse, sread text parse time routine";
        }

        public override void Execute(ushort aCharBufferAddress, ushort aTokenBufferAddress, ushort aTimeoutSeconds, ushort aTimeoutCallback, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            Machine.Input.Read(aCharBufferAddress, aTokenBufferAddress, aTimeoutSeconds, aTimeoutCallback);
        }
    }
}
