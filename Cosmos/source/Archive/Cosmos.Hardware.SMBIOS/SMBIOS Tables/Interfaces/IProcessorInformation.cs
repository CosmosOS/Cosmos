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
    public interface IProcessorInformation : IHardwareType 
    {
        string SocketDesignation { get; }
        byte ProcessorType { get; }
        byte ProcessorFamily { get; }
        string ProcessorManufacturer { get; }
        ulong ProcessorID { get; }
        string ProcessorVersion { get; }
        byte Voltage { get; }
        ushort ExternalClock { get; }
        ushort MaxSpeed { get; }
        ushort CurrentSpeed { get; }
        byte Status { get; }
        byte ProcessorUpgrade { get; }
        ushort L1CacheHandle { get; }
        ushort L2CacheHandle { get; }
        ushort L3CacheHandle { get; }
        string SerialNumber { get; }
        string AssetTag { get; }
        string PartNumber { get; }
        byte CoreCount { get; }
        byte CoreEnabled { get; }
        byte ThreadCount { get; }
        ushort ProcessorCharacteristics { get; }
        ushort ProcessorFamily2 { get; }
    }
}