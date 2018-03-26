using System;

namespace Cosmos.Core.SMBIOS
{
    public unsafe class BIOSInfo : SMBIOSTable
    {
        private byte VendorID = 0xff;
        private byte VersionID = 0xff;
        private byte ReleaseDateID = 0xff;
        private EntryPointTable EntryPointTable;

        public string Vendor{ get; set; }
        public string Version { get; set; }
        public ushort StartingAddressSegment { get; set; }
        public string ReleaseDate { get; set; }
        //TODO: do the calculation explained in the reference
        public byte ROMSize { get; set; }
        //Since its qword its a int64
        public ulong Characteristics { get; set; }
        public byte[] OptionalCharacteristics { get; set; }
        public byte SystemBiosMajorRelease { get; set; }
        public byte SystemBiosMinorRelease { get; set; }
        public byte EmbeddedControllerFirmwareMajorRelease { get; set; }
        public byte EmbeddedControllerFirmwareMinorRelease { get; set; }
        public ushort ExtendedBiosROMSize { get; set; }

        //We asume that the smbios is always greater than 2.0
        //TODO: independice the hardcoded numbers with a variable (which we will use to move through memory)
        public BIOSInfo(EntryPointTable entryPointTable, byte* BeginningAddress) : base(BeginningAddress)
        {
            this.BeginningAddress = BeginningAddress;
            this.EntryPointTable = entryPointTable;
        }

        //We go byte by byte MANUALLY to parse the table.
        //The field that is assigned is autodocumented
        //We use the BitConverter for words and qwords (2 bytes and 8 bytes, respectively);
        public override byte* Parse()
        {
            byte* newAddress =  BeginningAddress;
            int i;
            int j;

            //Begin the header parse
            this.Type = BeginningAddress[0];
            this.Length = BeginningAddress[1];
            byte[] tmp = new byte[2];
            tmp[0] = BeginningAddress[2];
            tmp[1] = BeginningAddress[3];
            this.Handle = BitConverter.ToUInt16(tmp, 0);

            //Create a new byte array in which we will do the parsing 
            //This array will contain the formatted section of the table 
            byte[] parseArray = new byte[Convert.ToInt32(this.Length)];
            for (int k = 0; k < this.Length; k++)
            {
                //Copy the formatted section byte to byte
                parseArray[k] = BeginningAddress[k];
            }
            

            //Start parsing the formatted section using the previously created array
            //We do a 'best effort parse' which means that we parse until we go out of bounds of the array
            //Then we finish with an exception
            //This has two main advantages:
            //1. No need to check for versions since we do the parsing in function of the length
            //2. The parsing is quite robust since we do using the length parameter (which we assume it is 
            //always right)
            //TODO: method for this try
            try
            {
                VendorID = parseArray[4];
                VersionID = parseArray[5];

                tmp[0] = parseArray[6];
                tmp[1] = parseArray[7];
                StartingAddressSegment = BitConverter.ToUInt16(tmp, 0);

                ReleaseDateID = parseArray[8];
                ROMSize = parseArray[9];

                tmp = new byte[8];
                for (int k = 0; k < 8; k++)
                {
                    //Since we left in 10...
                    tmp[k] = parseArray[k + 10];
                }
                Characteristics = BitConverter.ToUInt64(tmp, 0);

                //Begin to parse the optional characteristics
                //Since it is an optional field, we need to calculate its size first
                //Formula: Length - 12h == Length - 18
                //var size = Length - 18;

                // I dont know if the specification is incorrect but i count 22 bytes, not 18 (you must 
                // count the system bios bytes and firmware (since they are 22 bytes).
                // Might be a misintrepretation of the specification
                var size = Length - 22;
                // If there is no optional characteristic, skip
                if (size > 0)
                {
                    OptionalCharacteristics = new byte[size];
                    //We start whre we left (18)
                    for (int k = 0; k < size; k++)
                    {
                        OptionalCharacteristics[k] = parseArray[k + 18];
                    }
                }

                SystemBiosMajorRelease = parseArray[size + 18];
                SystemBiosMinorRelease = parseArray[size + 19];
                EmbeddedControllerFirmwareMajorRelease = parseArray[size + 20];
                EmbeddedControllerFirmwareMinorRelease = parseArray[size + 21];

                //This will not work in bochs since its version is 2.4
                size += 2;
                tmp = new byte[2];
                tmp[0] = parseArray[size + 22];
                tmp[1] = parseArray[size + 23];
                ExtendedBiosROMSize = BitConverter.ToUInt16(tmp, 0);
            }
            catch (IndexOutOfRangeException ex) { }

            //We have finished parsing the formatted area so we need to recompute the pointer
            //We start now the unformatted area
            //NOTE: we cannot sum this.Length directly. Gives IL2CPU error.
            newAddress = BeginningAddress + Convert.ToInt32(this.Length);

            //Parse the first string
            int[] tmpArray = new int[3];
            tmpArray[0] = VendorID;
            tmpArray[1] = ReleaseDateID;
            tmpArray[2] = VersionID;
            //TODO: method for this
            for (int q = 0; q < 3; q++)
            {
                for (int w = 1; w < 3 - q; w++)
                {
                    if (tmpArray[w - 1] > tmpArray[w])
                    {
                        var tmp2 =  tmpArray[w - 1];
                        tmpArray[w - 1] = tmpArray[w];
                        tmpArray[w] = tmp2;
                    }
                } 
            }
            //Array of strings from the formatted section
            string[] stringArray = SMBIOS.ParseStrings(newAddress);
            int iteration = -1;
            foreach (int t in tmpArray)
            {
                if (t == 255 | t == 0)
                    continue;
                //We increment the index first so the first string is 0
                //The numbers doesn't have to be correlative i.e
                //Doesn;'t have to be 1,2,3 it could be 1,2,15
                //Thus, we cannot use t to index the array
                iteration++;
                if (t == VendorID)
                    Vendor = stringArray[iteration];
                else if (t == ReleaseDateID)
                    ReleaseDate = stringArray[iteration];
                else if (t == VersionID)
                    Version = stringArray[iteration];
                else
                    continue;
            }

            //We need to recompute the pointer after parsing.
            return SMBIOS.RecomputePointer(newAddress, stringArray);
        }
    }
}

