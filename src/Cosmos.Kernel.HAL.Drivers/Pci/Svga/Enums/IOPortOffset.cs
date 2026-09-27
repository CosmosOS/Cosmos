using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

/// <summary>
/// IO port offset.
/// </summary>
[Experimental("COSMOS0003")]
public enum IOPortOffset : byte
{
    /// <summary>
    /// Index.
    /// </summary>
    Index = 0,
    /// <summary>
    /// Value.
    /// </summary>
    Value = 1,
    /// <summary>
    /// BIOS.
    /// </summary>
    Bios = 2,
    /// <summary>
    /// IRQ.
    /// </summary>
    IRQ = 3
}
