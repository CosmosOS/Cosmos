// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>
/// IO port offset.
/// </summary>
internal enum IOPortOffset : byte
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
