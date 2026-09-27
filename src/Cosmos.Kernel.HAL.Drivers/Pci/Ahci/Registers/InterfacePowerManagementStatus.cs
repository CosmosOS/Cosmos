// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;

/// <summary>
/// PxSSTS.IPM: the interface power management state.
/// </summary>
internal enum InterfacePowerManagementStatus : uint
{
    /// <summary>The interface is in the active state.</summary>
    Active = 0x01
}
