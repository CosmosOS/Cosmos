using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
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
        }

        public override unsafe byte* Parse()
        {
            byte* currentAddress;

            //We need a function to do parse a word
            Type = beginningAddress[0];
            Length = beginningAddress[1];

            byte[] tmp = new byte[2];
            tmp[0] = beginningAddress[2];
            tmp[1] = beginningAddress[3];
            Handle = BitConverter.ToUInt16(tmp, 0);

            SocketDesignationID = beginningAddress[4];
            ProcessorType = beginningAddress[5];
            ProcessorFamily = beginningAddress[6];
            ProcessorManufacturerID = beginningAddress[7];

            tmp = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                tmp[i] = beginningAddress[i + 8];
            }
            ProcessorID = BitConverter.ToUInt64(tmp, 0);
            

            ProcessorVersionID = beginningAddress[16];
            Voltage = beginningAddress[17];

            tmp = new byte[2];
            tmp[0] = beginningAddress[18];
            tmp[1] = beginningAddress[19];
            ExternalClock = BitConverter.ToUInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = beginningAddress[20];
            tmp[1] = beginningAddress[21];
            MaxSpeed = BitConverter.ToUInt16(tmp, 0);

            tmp = new byte[2];
            tmp[0] = beginningAddress[22];
            tmp[1] = beginningAddress[23];
            CurrentSpeed = BitConverter.ToUInt16(tmp, 0);

            Status = beginningAddress[24];
            ProcessorUpgrade = beginningAddress[25];


            if (entryPointTable.IsVersionGreaterThan(2, 1))
            {
                tmp = new byte[2];
                tmp[0] = beginningAddress[26];
                tmp[1] = beginningAddress[27];
                L1HandleCache = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = beginningAddress[28];
                tmp[1] = beginningAddress[29];
                L2HandleCache = BitConverter.ToUInt16(tmp, 0);

                tmp = new byte[2];
                tmp[0] = beginningAddress[30];
                tmp[1] = beginningAddress[31];
                L3HandleCache = BitConverter.ToUInt16(tmp, 0);


                if (entryPointTable.IsVersionGreaterThan(2, 3))
                {
                    SerialNumberID = beginningAddress[32];
                    AssetTagID = beginningAddress[33];
                    PartNumberID = beginningAddress[34];


                    if (entryPointTable.IsVersionGreaterThan(2, 5))
                    {
                        CoreCount = beginningAddress[35];
                        CoreEnabled = beginningAddress[36];
                        ThreadCount = beginningAddress[37];
    
                        tmp = new byte[2];
                        tmp[0] = beginningAddress[38];
                        tmp[1] = beginningAddress[39];
                        ProcessorCharacteristics = BitConverter.ToUInt16(tmp, 0);


                        if (entryPointTable.IsVersionGreaterThan(2, 6))
                        {
                            tmp = new byte[2];
                            tmp[0] = beginningAddress[40];
                            tmp[1] = beginningAddress[41];
                            ProcessorFamily2 = BitConverter.ToUInt16(tmp, 0);


                            if (entryPointTable.IsVersionGreaterThan(3, 0))
                            {
                                tmp = new byte[2];
                                tmp[0] = beginningAddress[42];
                                tmp[1] = beginningAddress[43];
                                CoreCount2 = BitConverter.ToUInt16(tmp, 0);

                                tmp = new byte[2];
                                tmp[0] = beginningAddress[44];
                                tmp[1] = beginningAddress[45];
                                CoreEnabled2 = BitConverter.ToUInt16(tmp, 0);

                                tmp = new byte[2];
                                tmp[0] = beginningAddress[46];
                                tmp[1] = beginningAddress[47];
                                ThreadCount2 = BitConverter.ToUInt16(tmp, 0);
                            }
                        }
                    }
                }
            }

            //We substract one so as not to skip one string
            //This is really fucked up in bochs but works nice in vmware.
            currentAddress = beginningAddress + Convert.ToInt32(Length) - 1;

            currentAddress = ParseStrings(currentAddress);

            return currentAddress;
        }


        public byte* ParseStrings(byte* beginningAddress)
        {
            var current = CompareStringN(beginningAddress, 0);
            int[] arr = new int[8];
            arr[0] = AssetTagID;
            arr[1] = PartNumberID;
            arr[2] = SerialNumberID;
            arr[3] = ProcessorManufacturerID;
            arr[4] = SerialNumberID;
            arr[5] = ProcessorVersionID;
            arr[6] = SocketDesignationID;
            arr[7] = PartNumberID;
            //Some shitty bubble sort
            //Array.Sort goes shit
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
            for (int i = 0; i < 8; i++)
            {
                if (arr[i] == 0 || arr[i] == 255)
                {
                    continue;
                }
                else
                {
                    current = CompareStringN(current, arr[i]);
                }
                
            }
            return current;
        }

        public byte* CompareStringN(byte* beginningAddress, int position)
        {
            string var;
            var memPtr = Core.SMBIOS.SMBIOS.ParseString(beginningAddress, out var);
            //I'm not able to work out a switch here (wtf)
            if (AssetTagID == position)
                AssetTag = var;
            else if (PartNumberID == position)
                PartNumber = var;
            else if (ProcessorManufacturerID == position)
                ProcessorManufacturer = var;
            else if (ProcessorVersionID == position)
                ProcessorVersion = var;
            else if (SerialNumberID == position)
                SerialNumber = var;
            else if (SocketDesignationID == position)
                SocketDesignation = var;
            else return beginningAddress; //We do as we did nothing

            return memPtr; //If everything goes well this method should end here
        }
    }
}
