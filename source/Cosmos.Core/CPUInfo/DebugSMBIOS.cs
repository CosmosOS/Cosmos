using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core.CPUInfo
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
            Debugger.DoSend("Vendor: " + bios.VendorID);
            Debugger.DoSend("Bios Version: " + bios.VersionID);
            Debugger.DoSend("Bios Starting Address Segment: " + bios.StartingAddressSegment);
            Debugger.DoSend("Bios release date: " + bios.ReleaseDateID);
            Debugger.DoSend("Bios rom size: " + 64 * (bios.ROMSize + 1) + "K");
            Debugger.DoSend("Bios characteristics: " + bios.Characteristics);
            Debugger.DoSend("Bios extension bytes characteristics (length): " + bios.OptionalCharacteristics.Length);
            Debugger.DoSend("System bios release: " + bios.SystemBiosMajorRelease + "." + bios.SystemBiosMinorRelease);
            Debugger.DoSend("Firmware release: " + bios.EmbeddedControllerFirmwareMajorRelease + "." + bios.EmbeddedControllerFirmwareMinorRelease);
            Debugger.DoSend("Extended bios rom size: " + bios.ExtendedBiosROMSize);
        }
    }
}
