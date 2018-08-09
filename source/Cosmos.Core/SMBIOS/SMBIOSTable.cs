namespace Cosmos.Core.SMBIOS
{
    /// <summary>
    /// This class acts as a base layer for each type of smbios table
    /// </summary>
    public unsafe abstract class SMBIOSTable
    {
        public byte* BeginningAddress { get; set; }
        public byte Type { get; set; }
        public byte Length { get; set; }
        public ushort Handle { get; set; }

        protected SMBIOSTable(byte* BeginningAddress)
        {
        }

        public abstract byte* Parse();
    }
}
