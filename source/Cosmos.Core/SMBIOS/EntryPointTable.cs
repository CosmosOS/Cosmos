using System;

namespace Cosmos.Core.SMBIOS
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
            if (this.MajorVersion > majorVersion)
            {
                return true;
            }
            else if(this.MajorVersion == majorVersion)
            {
                if (this.MinorVersion >= minorVersion)
                {
                    return true;
                }
            }
            return false;
        }

        public void Parse(byte* memPtr)
        {
            EntryPointAddr = memPtr;

            //Parse the _SM_
            for (int i = 0; i < 4; i++)
            {
                EntryPointString[i] = (char)memPtr[i];
            }

            CheckSum = memPtr[4];
            Length = memPtr[5];
            MajorVersion = memPtr[6];
            MinorVersion = memPtr[7];

            //Create the array of byte to cast to ushort (to cast the maxStructureSize)
            byte[] tmpArray = new byte[2];
            tmpArray[0] = memPtr[8];
            tmpArray[1] = memPtr[9];
            MaxStructureSize = BitConverter.ToUInt16(tmpArray, 0);

            //Continue parsing the table...
            EntryPointRevision = memPtr[10];
            for (int i = 0; i < 5; i++)
            {
                FormattedArea[i] = (char)memPtr[i + 11];
            }
            for (int i = 0; i < 5; i++)
            {
                EntryPointString2[i] = (char) memPtr[i + 16];
            }

            CheckSum2 = memPtr[21];
            tmpArray = new byte[2];
            tmpArray[0] = memPtr[22];
            tmpArray[1] = memPtr[23];
            TableLength = BitConverter.ToUInt16(tmpArray, 0);

            //Get the pointer to the main table
            // 4 bytes = 32 bits = address
            tmpArray = new byte[4];
            tmpArray[0] = memPtr[24];
            tmpArray[1] = memPtr[25];
            tmpArray[2] = memPtr[26];
            tmpArray[3] = memPtr[27];
            TableAddress = (uint*)BitConverter.ToUInt32(tmpArray, 0);

            //Get the number of structures
            tmpArray = new byte[2];
            tmpArray[0] = memPtr[28];
            tmpArray[1] = memPtr[29];
            NumberOfStructures = BitConverter.ToUInt16(tmpArray, 0);

            BCDRevision = memPtr[30];
        }


    }
}
