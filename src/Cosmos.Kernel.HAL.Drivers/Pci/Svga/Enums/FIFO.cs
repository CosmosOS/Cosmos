using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Svga;

/// <summary>
/// FIFO values.
/// </summary>
[Experimental("COSMOS0003")]
public enum FIFO : uint
{   // values are multiplied by 4 to access the array by byte index
    /// <summary>
    /// Min.
    /// </summary>
    Min = 0,
    /// <summary>
    /// Max.
    /// </summary>
    Max = 4,
    /// <summary>
    /// Next command.
    /// </summary>
    NextCmd = 8,
    /// <summary>
    /// Stop.
    /// </summary>
    Stop = 12
}
