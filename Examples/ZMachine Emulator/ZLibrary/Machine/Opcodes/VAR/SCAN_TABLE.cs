namespace ZLibrary.Machine.Opcodes.VAR
{
    /// <summary>
    /// Find and store the address of a target within a table.
    /// </summary>
    public class SCAN_TABLE : Opcode
    {
        public SCAN_TABLE(ZMachine aMachine)
            : base(aMachine)
        {
            Name = "VAR:0x17 scan_table x table len form -> (result)";
        }

        public override void Execute(ushort aValue, ushort aTableAddress, ushort aScanLength, ushort aType, ushort aArg4, ushort aArg5, ushort aArg6, ushort aArg7, ushort aArgCount)
        {
            bool finished = false;
            ushort xCurrentAddress = aTableAddress;

            for (int i = 0; i < aScanLength; i++)
            {

                if ((aType & 0x80) > 0)
                {
                    ushort wvalue;

                    Machine.Memory.GetWord(xCurrentAddress, out wvalue);

                    if (wvalue == aValue)
                    {
                        finished = true;
                        break;
                    }
                }
                else
                {
                    byte bvalue;

                    Machine.Memory.GetByte(xCurrentAddress, out bvalue);

                    if (bvalue == aValue)
                    {
                        finished = true;
                        break;
                    }
                }

                xCurrentAddress += (ushort)(aType & 0x7f);
            }

            if (!finished)
            {
                xCurrentAddress = 0;
            }

            Store(xCurrentAddress);
            Branch(xCurrentAddress > 0);
        }
    }
}
