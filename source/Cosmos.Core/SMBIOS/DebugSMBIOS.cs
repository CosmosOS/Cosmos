using Cosmos.Debug.Kernel;

namespace Cosmos.Core.SMBIOS
{
    public unsafe class DebugSMBIOS
    {
        public static void DebugEntryPoint(EntryPointTable entry)
        {
            Debugger.DoSend("TEST PARSE");

            Debugger.DoSend(entry.EntryPointString[0].ToString() + entry.EntryPointString[1] + entry.EntryPointString[2] + entry.EntryPointString[3]);
            Debugger.DoSend("Revision: " + entry.EntryPointRevision);
            Debugger.DoSend("Version: " + entry.MajorVersion + "." + entry.MinorVersion);
            Debugger.DoSend("Max structure size: " + entry.MaxStructureSize);
            Debugger.DoSend("Formatted area: " + entry.FormattedArea[0].ToString() +
                entry.FormattedArea[1] +
                entry.FormattedArea[2] +
                entry.FormattedArea[3] +
                entry.FormattedArea[4]
                );
            Debugger.DoSend("Entry point string 2: " + entry.EntryPointString2[0].ToString() +
                entry.EntryPointString2[1] +
                entry.EntryPointString2[2] +
                entry.EntryPointString2[3] +
                entry.EntryPointString2[4]
                );
            Debugger.DoSend("Number of structures:" + entry.NumberOfStructures);

            Debugger.DoSend("Structures length: " + entry.TableLength);
            Debugger.DoSend("First table: " + (byte)entry.TableAddress[0]);
            Debugger.DoSend("Contents: ");
        }

        public static void DebugBIOSInfo(BIOSInfo bios)
        {
            Debugger.DoSend("------------BIOS INFO-------------");
            Debugger.DoSend("Type: " + bios.Type); 
            Debugger.DoSend("Legnth: " + bios.Length);
            Debugger.DoSend("Handle: " + bios.Handle);
            Debugger.DoSend("Bios Starting Address Segment: " + bios.StartingAddressSegment);
            Debugger.DoSend("Bios rom size: " + 64 * (bios.ROMSize + 1) + "K");
            Debugger.DoSend("Bios characteristics: " + bios.Characteristics);
            Debugger.DoSend("Bios extension bytes characteristics (length): " + bios.OptionalCharacteristics.Length);
            Debugger.DoSend("System bios release: " + bios.SystemBiosMajorRelease + "." + bios.SystemBiosMinorRelease);
            Debugger.DoSend("Firmware release: " + bios.EmbeddedControllerFirmwareMajorRelease + "." + bios.EmbeddedControllerFirmwareMinorRelease);
            Debugger.DoSend("Extended bios rom size: " + bios.ExtendedBiosROMSize);
            Debugger.DoSend("Vendor: " + bios.Vendor);
            Debugger.DoSend("Release date: " + bios.ReleaseDate);
            Debugger.DoSend("Version: " + bios.Version);
        }

        public static void DebugCPUInfo(CPUInfo cpuInfo)
        {
            Debugger.DoSend("==================CPU==========================");
            Debugger.DoSend("Type:" + cpuInfo.Type); 
            Debugger.DoSend("Length: " + cpuInfo.Length);
            Debugger.DoSend("Handle: " + cpuInfo.Handle);
            Debugger.DoSend("Socket designation (ID): " + cpuInfo.SocketDesignationID);
            Debugger.DoSend("Processor Type: " + cpuInfo.ProcessorType);
            Debugger.DoSend("Processor family: " + cpuInfo.ProcessorFamily);
            Debugger.DoSend("Processor manufacturer (ID): " + cpuInfo.ProcessorManufacturerID);
            Debugger.DoSend("Processor ID: " + cpuInfo.ProcessorID);
            Debugger.DoSend("Processor version (ID): " + cpuInfo.ProcessorVersionID);
            Debugger.DoSend("Voltage: " + cpuInfo.Voltage);
            Debugger.DoSend("External clock: " + cpuInfo.ExternalClock);
            Debugger.DoSend("Max Speed: " + cpuInfo.MaxSpeed + " MHZ");
            Debugger.DoSend("Current speed: " + cpuInfo.CurrentSpeed + " MHZ");
            Debugger.DoSend("Status: " + cpuInfo.Status);
            Debugger.DoSend("Processor upgrade: " + cpuInfo.ProcessorUpgrade);
            Debugger.DoSend("L1 Cache Handle: " + cpuInfo.L1HandleCache);
            Debugger.DoSend("L2 Cache Handle: " + cpuInfo.L2HandleCache);
            Debugger.DoSend("L3 Cache Handle: " + cpuInfo.L3HandleCache); 
            Debugger.DoSend("Serial number (ID): " + cpuInfo.SerialNumberID);
            Debugger.DoSend("Asset Tag (ID): " + cpuInfo.AssetTagID);
            Debugger.DoSend("Par number (ID): " + cpuInfo.PartNumberID);
            Debugger.DoSend("Core count: " + cpuInfo.CoreCount);
            Debugger.DoSend("Core enabled: " + cpuInfo.CoreEnabled);
            Debugger.DoSend("Thread Count: " + cpuInfo.ThreadCount);
            Debugger.DoSend("Processor characteristics: " + cpuInfo.ProcessorCharacteristics);
            Debugger.DoSend("Processor family 2: " + cpuInfo.ProcessorFamily2);
            Debugger.DoSend("  ===========STRINGS==========");
            Debugger.DoSend("Asset Tag: " + cpuInfo.AssetTag);
            Debugger.DoSend("PartNumber: " + cpuInfo.PartNumber);
            Debugger.DoSend("ProcessorManufacturer: " + cpuInfo.ProcessorManufacturer);
            Debugger.DoSend("ProcessorVersion: " + cpuInfo.ProcessorVersion);
            Debugger.DoSend("SerialNumber: " + cpuInfo.SerialNumber);
            Debugger.DoSend("SocketDesignation: " + cpuInfo.SocketDesignation);
            Debugger.DoSend("Part Number: " + cpuInfo.PartNumber);
            Debugger.DoSend("  ===================================================\n");
        }
    }
}