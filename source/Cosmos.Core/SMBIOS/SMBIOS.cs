using System.Collections.Generic;

namespace Cosmos.Core.SMBIOS
{

    //This class contains the parser for the entry point table.
    //TODO: do checks for versions.
    public unsafe class SMBIOS
    {
        public static SMBIOSStructure BeginParseSMBIOS()
        {
            byte* memPtr = SMBIOS.SearchEntryPointTable();

            EntryPointTable entry =  new EntryPointTable();
            //We dont return an address since we need to use a pointer that
            //its inside the table
            entry.Parse(memPtr);

            //entry.GetTableAddress();
            SMBIOSStructure smbiosStructure = SMBIOS.ParseStructures(entry);
            smbiosStructure.EntryPointTable = entry;
            return smbiosStructure;
        }

        /// <summary>
        /// This fucntion parses the array of strings of the unformatted part of the structure
        /// Stops when found the end of the table. THE ADDRESS NEEDS TO BE RECOMPUTED.
        /// The start of the next table needs to be recomputed as the sum of the length of the parsed strings;
        /// NOTE: assumes that the start of the unformatted section will be provided
        /// </summary>
        /// <param name="beginningAddress">Address in which we start searching</param>
        /// <param name="variable">Variable in which we will store the result</param>
        /// <returns>Offset of the search (i.e, the number of position searched)</returns>
        public static string[] ParseStrings(byte* beginningAddress)
        {
            List<string> stringList = new List<string>();
            var i = 0;
            //While we don't find the double null...
            while (beginningAddress[i] != '\0')
            {
                string parsedString = "";
                //While we don't find the null that indicates the end of the string...
                while (beginningAddress[i] != '\0')
                {
                    //Create new string appending chars
                    parsedString = parsedString + ((char)beginningAddress[i]);
                    i++;
                }
                stringList.Add(parsedString);
                i++; //Skip the first null or the null that indicates end of string
            }
            return stringList.ToArray();
        }

        /// <summary>
        /// Recomputes the pointer after parsing the strings of the unformatted section.
        /// </summary>
        /// <param name="beginningAddress">Address after parsing the formatted section</param>
        /// <param name="stringArray">Array of strings parsed from the unformatted section</param>
        /// <returns>Starting address of the next table</returns>
        public static byte* RecomputePointer(byte* beginningAddress, string[] stringArray)
        {
            int charLength = 0; //sum of the length of the strings contained in stringArray
            foreach (var str in stringArray)
                charLength += str.Length;
            //The next address will be
            return beginningAddress +
                   charLength + //The sum of the length of all the strings
                   stringArray.Length + //The number of null bytes related to the strings
                   1; //The second null byte in the double null part
        }
    

        public static SMBIOSStructure ParseStructures(EntryPointTable entryPointTable)
        {
            SMBIOSStructure smbiosStructure = new SMBIOSStructure();
            List<CPUInfo> cpuList = new List<CPUInfo>();
            byte* currentAddress = entryPointTable.GetTableAddress();

            DebugSMBIOS.DebugEntryPoint(entryPointTable);
            for (int i = 0; i < entryPointTable.NumberOfStructures; i++)
            {
                //We need to compare the type (which will be always the 0 fo current address)
                if (currentAddress[0] == SMBIOSTypes.BiosTable)
                {
                        if (smbiosStructure.BiosInfo == null)
                        {
                            smbiosStructure.BiosInfo = new BIOSInfo(entryPointTable, currentAddress);
                            currentAddress = smbiosStructure.BiosInfo.Parse();
                            DebugSMBIOS.DebugBIOSInfo(smbiosStructure.BiosInfo);
                        }
                        else
                        {
                            //If we fail skipping the table
                            currentAddress = currentAddress + 1;
                            Cosmos.Debug.Kernel.Debugger.DoSend("Skipping not bios table");
                        }
                    continue;
                }
                if (currentAddress[0] == SMBIOSTypes.ProcessorTable)
                {
                    CPUInfo cpuInfo = new CPUInfo(entryPointTable, currentAddress);
                    currentAddress = cpuInfo.Parse();
                    smbiosStructure.CpuInfoList.Add(cpuInfo);
                    DebugSMBIOS.DebugCPUInfo(cpuInfo);
                    continue;
                }
                //In [1] we have the length of the formatted section.
                Cosmos.Debug.Kernel.Debugger.DoSend("Skipping table type: " + currentAddress[0] + " Length: " + currentAddress[1]);
                Cosmos.Debug.Kernel.Debugger.DoSend("Is 4?" + (currentAddress[0] == 4));
                currentAddress = SkipTable(currentAddress[1], currentAddress);
            }
            return smbiosStructure;
        }

        /// <summary>
        /// Skip a table with length x
        /// We need the length since the table can contain double nulls inside the formatted section
        /// </summary>
        /// <param name="length">Length of the table to skip</param>
        /// <param name="beginningAddress">Address to search</param>
        /// <returns></returns>
        public static byte* SkipTable(int length, byte* beginningAddress)
        {
            int i;
            //Skip the formatted section
            for (i = 0; i < length; i++) ;

            //Skip the unformatted section (bunch of strings)
            for (;;)
            {
                //If we found the double null we finished the table
                if (beginningAddress[i] == 0 && beginningAddress[i + 1] == 0)
                {
                    //The pointer will be situated under the first null
                    //We sum 2 to skip this
                    return beginningAddress + i + 2;
                }
                //We need to increment here.
                //If we increment before checking for double null it doesnt work
                //for tables without strings
                i++;
            }

        }

        /// <summary>
        /// This method searches the memory region so as to locate the
        /// "Entry Point Table"
        /// </summary>
        /// <returns>Pointer to the beggining of the table</returns>
        public static byte* SearchEntryPointTable()
        {
            string str = "";
            byte* memPtr;
            int length, checksum;
            for (memPtr = (byte*) 0xf0000;
                memPtr < (byte*)0x100000;
                memPtr += 16)
            {
                if (memPtr[0] == '_' && memPtr[1] == 'S' && memPtr[2] == 'M' && memPtr[3] == '_')
                {

                    //TODO: do checksum
                    /*
                    length = memPtr[5];
                    checksum = 0;
                    for (int i = 0; i < length; i++)
                    {
                        checksum += memPtr[i];
                        str += (char)memPtr[i];
                    }
                    if (checksum == 0)
                    */
                    return memPtr;
                }
            }
            return null;

        }
    }
}
