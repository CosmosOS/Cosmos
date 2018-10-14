namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Copy a table or fill it with zeroes.
    /// </summary>
    public class COPY_TABLE : Opcode
    {
        public COPY_TABLE(ZMachine machine)
            : base(machine)
        {
            Name = "VAR:0x1D copy_table first second size";
        }

        public override void Execute(ushort aTableAddress, ushort aDestinationAddress, ushort aLength, ushort aArg3, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            ushort addr;
            byte value;
            int i;

            if (aDestinationAddress == 0)
            {
                for (i = 0; i < aLength; i++)
                {
                    Machine.Memory.SetByte((aTableAddress + i), 0);
                }
            }
            else if ((short)aLength < 0 || aTableAddress > aDestinationAddress)
            {
                for (i = 0; i < (((short)aLength < 0) ? -(short)aLength : aLength); i++)
                {
                    addr = (ushort)(aTableAddress + i);
                    Machine.Memory.GetByte(addr, out value);
                    Machine.Memory.SetByte((aDestinationAddress + i), value);
                }
            }
            else
            {
                for (i = aLength - 1; i >= 0; i--)
                {
                    addr = (ushort)(aTableAddress + i);
                    Machine.Memory.GetByte(addr, out value);
                    Machine.Memory.SetByte((aDestinationAddress + i), value);
                }
            }
        }
    }
}
