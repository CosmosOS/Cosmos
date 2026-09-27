// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci.Registers;

/// <summary>
/// PxIS bits this driver reads (AHCI 1.3.1 s3.3.5).
/// </summary>
[Flags]
internal enum InterruptStatus : uint
{
    /// <summary>TFES: the device reported an error in the task file.</summary>
    TaskFileErrorStatus = 1u << 30
}
