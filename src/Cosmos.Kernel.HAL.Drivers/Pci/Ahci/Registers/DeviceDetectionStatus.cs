// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;

/// <summary>
/// PxSSTS.DET: device detection and PHY state.
/// </summary>
internal enum DeviceDetectionStatus : uint
{
    /// <summary>A device is present and PHY communication is established.</summary>
    DeviceDetectedWithPhy = 0x03
}
