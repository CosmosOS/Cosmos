using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Core.CPUInfo
{
    public unsafe class EntryPointTable
    {
        //Address of the beggining of the table
        public byte* EntryPointAddr { get; set; }
        public char[] EntryPointString { get; set; }   
        public byte CheckSum { get; set; }
        public byte Length { get; set; }
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public ushort MaxStructureSize { get; set; }
        public byte EntryPointRevision { get; set; }
        // Size = 5
        public char[] FormattedArea{ get; set; }
        // _DMI_
        public char[] EntryPointString2 { get; set; }
        public byte CheckSum2 { get; set; }
        public ushort TableLength { get; set; }
        //Address of the read-only SMBIOS Structure Table
        public uint* TableAddress { get; set; }
        public ushort NumberOfStructures { get; set; }
        public byte BCDRevision { get; set; }

        public EntryPointTable()
        {
            FormattedArea = new char[5];
            EntryPointString = new char[4];
            EntryPointString2 = new char[5];
        }

        /// <summary>
        /// Converts the table address (TableAddress) from uint* to byte* (easier to use)
        /// </summary>
        public byte* GetTableAddress()
        {
            return ((byte*) TableAddress);
        }

        /// <summary>
        /// Checks if the version is equal or greater than the provided version
        /// </summary>
        /// <param name="majorVersion">Entire part of the version</param>
        /// <param name="">Decimal part of the version</param>
        /// <returns></returns>
        public bool IsVersionGreaterThan(int majorVersion, int minorVersion)
        {
            if (majorVersion >= this.MajorVersion)
            {
                if (minorVersion >= this.MinorVersion)
                {
                    return true;
                }
            }
            return false;
        }


    }
}
