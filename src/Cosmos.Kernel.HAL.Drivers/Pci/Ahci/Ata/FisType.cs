// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Ata;

/// <summary>
/// FIS (Frame Information Structure) types this driver builds.
/// </summary>
internal enum FisType : byte
{
    /// <summary>Register FIS: Host to Device.</summary>
    RegisterH2D = 0x27
}
