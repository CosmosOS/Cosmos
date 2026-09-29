// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>
/// FIFO values.
/// </summary>
internal enum FIFO : uint
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
