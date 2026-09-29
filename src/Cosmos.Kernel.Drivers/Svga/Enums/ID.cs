// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Svga;

/// <summary>
/// ID values.
/// </summary>
internal enum ID : uint
{
    /// <summary>
    /// Magic starting point.
    /// </summary>
    Magic = 0x900000,
    /// <summary>
    /// V0.
    /// </summary>
    V0 = Magic << 8,
    /// <summary>
    /// V1.
    /// </summary>
    V1 = Magic << 8 | 1,
    /// <summary>
    /// V2.
    /// </summary>
    V2 = Magic << 8 | 2,
    /// <summary>
    /// Invalid
    /// </summary>
    Invalid = 0xFFFFFFFF
}
