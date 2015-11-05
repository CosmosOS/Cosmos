/* Licensed under the terms of the New BSD License.
*
* Authors:
*  Gero Landmann (gero_dev) <gero@myzeug.de>
*/
using System;
using System.Collections.Generic;

using Cosmos.Kernel;

namespace Cosmos.Hardware.SMBIOS
{
    /// <summary>
    /// 
    /// </summary>
    public enum TableTypes : int
    {
        BIOSInformation = 0,
        SystemInformation = 1,
        BaseBoardInformation = 2,
        SystemEnclosure = 3,
        ProcessorInformation = 4,
        CacheInformation = 7,
        PortConnectorInformation = 8,
        SystemSlots = 9,
        OEMStrings = 11,
        SystemConfigurationOptions = 12,
        BIOSLanguageInformation = 13,
        GroupAssociations = 14,
        SystemEventLog = 15,
        PhysicalMemoryArray = 16,
        MemoryDevice = 17,
        MemoryErrorInformation32Bit = 18,
        MemoryArrayMappedAddress = 19,
        MemoryDeviceMappedAddress = 20,
        BuiltInPointingDevice = 21,
        PortableBattery = 22,
        SystemReset = 23,
        HardwareSecurity = 24,
        SystemPowerControls = 25,
        VoltageProbe = 26,
        CoolingDevice = 27,
        TemperatureProbe = 28,
        ElectricalCurrentProbe = 29,
        OutOfBandRemoteAccess = 30,
        BootIntegrityServicesEntryPoint = 31,
        SystemBootInformation = 32,
        MemoryErrorInformation64Bit = 33,
        ManagementDevice = 34,
        ManagementDeviceComponent = 35,
        ManagementDeviceThresholdData = 36,
        MemoryChannel = 37,
        IPMIDeviceInformation = 38,
        SystemPowerSupply = 39,
        AdditionalInformation = 40,
        OnboardDevicesExtendedInformation = 41,
    }
}