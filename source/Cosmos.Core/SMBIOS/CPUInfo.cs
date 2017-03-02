using System;
using Cosmos.Core.PCInformation;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.SMBIOS
{

    public unsafe class CPUInfo : SMBIOSTable
    {
        private EntryPointTable entryPointTable;
        //TODO: convert to private
        public byte SocketDesignationID = 0xff;
        public byte ProcessorManufacturerID = 0xff;
        public byte ProcessorVersionID = 0xff;
        public byte SerialNumberID = 0xff;
        public byte AssetTagID = 0xff;
        public byte PartNumberID = 0xff;

        private byte* beginningAddress;
        

        public byte ProcessorType { get; set; }
        public byte ProcessorFamily { get; set; }
        /// <summary>
        /// This ID doesn't reference a string
        /// </summary>
        public ulong ProcessorID { get; set; } 
        public byte Voltage { get; set; }
        public ushort ExternalClock { get; set; }
        public ushort MaxSpeed { get; set; }
        public ushort CurrentSpeed { get; set; }
        public byte Status { get; set; } 
        public byte ProcessorUpgrade { get; set; }
        public ushort L1HandleCache { get; set; }
        public ushort L2HandleCache { get; set; }
        public ushort L3HandleCache { get; set; }
        public byte CoreCount { get; set; }
        public byte CoreEnabled { get; set; }
        public byte ThreadCount { get; set; }
        public ushort ProcessorCharacteristics { get; set; }
        public ushort ProcessorFamily2 { get; set; }
        public ushort CoreCount2 { get; set; }
        public ushort CoreEnabled2 { get; set; }
        public ushort ThreadCount2 { get; set; }

        //Gets the EAX register of CPUID (its part of ProcessorID)
        public uint CPUIDEAX { get; set; }
        //Get the EDX register of CPUID (it is part of ProcessorID)
        public uint CPUIDEDX { get; set; }


        public string SocketDesignation {get; set;}
        public string ProcessorManufacturer {get; set;}
        public string ProcessorVersion {get; set;}
        public string SerialNumber {get; set;}
        public string AssetTag {get; set;}
        public string PartNumber {get; set;}

        public CPUInfo(EntryPointTable entryPointTable, byte* BeginningAddress) : base(BeginningAddress)
        {
            this.beginningAddress = BeginningAddress;
            this.entryPointTable = entryPointTable;
            AssetTag = "";
            PartNumber = "";
            SerialNumber = "";
            ProcessorVersion = "";
            ProcessorManufacturer = "";
            SocketDesignation = "";
        }

        public override unsafe byte* Parse()
        {
            byte* currentAddress;
            //We need a function to do parse a word

            //Parse the header
            Type = beginningAddress[0];
            Length = beginningAddress[1];
            byte[] tmp = new byte[2];
            tmp[0] = beginningAddress[2];
            tmp[1] = beginningAddress[3];
            Handle = BitConverter.ToUInt16(tmp, 0);

            //Create the array in which we will do the parsing
            //Se biosInfo.cs comment

            //byte[] parseArray = new byte[Convert.ToInt32(this.Length)];
            byte[] parseArray = new byte[this.Length];
            for (int i = 0; i < this.Length; i++)
            {
                parseArray[i] = beginningAddress[i];
            }

            //Parse using parseArray

            try
            {
                SocketDesignationID = parseArray[4];
                ProcessorType = parseArray[5];
                ProcessorFamily = parseArray[6];
                ProcessorManufacturerID = parseArray[7];

                tmp = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    tmp[i] = parseArray[i + 8];
                }
                ProcessorID = BitConverter.ToUInt64(tmp, 0);

                //Processor id is the result of doing the CPUID instruction in x86
                //Processor ID (in x86) its compound of two parts
                //The first DWORD is the EAX part when the EAX part is put to 1
                //The second DWORD is the EDX part.
                //Store the EAX part of ProcessorID (since in x86 its as doing a CPUID instruction)
                tmp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    tmp[i] = parseArray[i + 8];
                }
                CPUIDEAX = BitConverter.ToUInt32(tmp, 0);
                //Store the EDX part of ProcessorID
                tmp = new byte[4];
                for (int i = 4; i < 8; i++)
                {
                    tmp[i - 4] = parseArray[i + 8];
                }
                CPUIDEDX = BitConverter.ToUInt32(tmp, 0);

                ProcessorVersionID = parseArray[16];
                Voltage = parseArray[17];

                tmp = new byte[2];
                tmp[0] = parseArray[18];
                tmp[1] = parseArray[19];
                ExternalClock = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[20];
                tmp[1] = parseArray[21];
                MaxSpeed = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[22];
                tmp[1] = parseArray[23];
                CurrentSpeed = BitConverter.ToUInt16(tmp, 0);

                Status = parseArray[24];
                ProcessorUpgrade = parseArray[25];


                tmp = new byte[2];
                tmp[0] = parseArray[26];
                tmp[1] = parseArray[27];
                L1HandleCache = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[28];
                tmp[1] = parseArray[29];
                L2HandleCache = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[30];
                tmp[1] = parseArray[31];
                L3HandleCache = BitConverter.ToUInt16(tmp, 0);

                SerialNumberID = parseArray[32];
                AssetTagID = parseArray[33];
                PartNumberID = parseArray[34];

                CoreCount = parseArray[35];
                CoreEnabled = parseArray[36];
                ThreadCount = parseArray[37];

                tmp = new byte[2];
                tmp[0] = parseArray[38];
                tmp[1] = parseArray[39];
                ProcessorCharacteristics = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[40];
                tmp[1] = parseArray[41];
                ProcessorFamily2 = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[42];
                tmp[1] = parseArray[43];
                CoreCount2 = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[44];
                tmp[1] = parseArray[45];
                CoreEnabled2 = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = parseArray[46];
                tmp[1] = parseArray[47];
                ThreadCount2 = BitConverter.ToUInt16(tmp, 0);
            }
            catch (IndexOutOfRangeException ex)
            {

            }

            currentAddress = beginningAddress + Convert.ToInt32(Length);

            var stringArray = SMBIOS.ParseStrings(currentAddress);
            StoreStrings(stringArray);

            return SMBIOS.RecomputePointer(currentAddress, stringArray);
        }


        /// <summary>
        /// Store the strings in their respective variables according to their id
        /// </summary>
        /// <param name="stringArray">Array of strings parsed from the SMBIOS</param>
        /// <returns></returns>
        public void StoreStrings(string[] stringArray)
        {
            int[] arr = new int[8];
            arr[0] = AssetTagID;
            arr[1] = PartNumberID;
            arr[2] = SerialNumberID;
            arr[3] = ProcessorManufacturerID;
            arr[4] = SerialNumberID;
            arr[5] = ProcessorVersionID;
            arr[6] = SocketDesignationID;
            arr[7] = PartNumberID;
            //Instead of doing an Array.Sort plug (too much responsability)
            //I will code the bubblesort here directly (besides, bubblesort its quite inneficient).
            for (int i = 0; i < 8; i++)
            {
                for (int j = 1; j < 8 - i; j++)
                {
                    if (arr[j - 1] > arr[j])
                    {
                        var tmp = arr[j - 1];
                        arr[j - 1] = arr[j];
                        arr[j] = tmp;
                    }
                }
            }
            for (int i = 0; i < arr.Length; i++)
            {
                Debugger.DoSend("arr: " + i + ", val: " + arr[i]);
            }
            foreach (var s in stringArray)
            {
                Debugger.DoSend("String: " + s); 
            }
            //We need to reference the string int the stringArray
            //We cannot use i since we need to skip some arr[i]
            int iteration = 0;
            for (int i = 0; i < 8; i++)
            {
                if (arr[i] == 0 || arr[i] == 255)
                    continue;
                else
                {
                    CompareStringN(arr[i], stringArray[iteration]);
                    iteration++;
                }

            }
        }

        /// <summary>
        /// Compares to the Nth string of the stringArray
        /// A simple switch
        /// </summary>
        /// <param name="position">TagID of the string obtained form SMBIOS</param>
        /// <param name="currentString">String to be stored in the table</param>
        public void CompareStringN(int position, string currentString)
        {
            //I'm not able to work out a switch here (wtf)
            if (AssetTagID == position)
                AssetTag = currentString;
            else if (PartNumberID == position)
                PartNumber = currentString;
            else if (ProcessorManufacturerID == position)
                ProcessorManufacturer = currentString;
            else if (ProcessorVersionID == position)
                ProcessorVersion = currentString;
            else if (SerialNumberID == position)
                SerialNumber = currentString;
            else if (SocketDesignationID == position)
                SocketDesignation = currentString;
            else
                return;
        }
    }
}
