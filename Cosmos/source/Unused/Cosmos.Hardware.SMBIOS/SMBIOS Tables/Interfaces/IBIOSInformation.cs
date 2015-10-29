/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/

namespace Cosmos.Hardware.SMBIOS
{
    /// <summary>
    /// Early intefacedefinition to get smbios working.
    /// changes definitly!
    /// </summary>
    public interface IBIOSInformation : IHardwareType 
    {
        string Vendor { get; }
        string BIOSVersion { get; }
        ushort BIOSStartingAddressSegment { get; }
        string BIOSReleaseDate { get; }
        byte BIOSROMSize { get; }
        ulong BIOSCharacteristics { get; }
        int BIOSCharacteristicsExtensionByteCount { get; }
        byte[] BIOSCharacteristicsExtensionBytes { get; }
        byte SystemBIOSMajorRelease { get; }
        byte SystemBIOSMinorRelease { get; }
        byte EmbeddedControllerFirmwareMajorRelease { get; }
        byte EmbeddedControllerFirmwareMinorRelease { get; }
    }
}