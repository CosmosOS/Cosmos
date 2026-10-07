// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The IP version an <see cref="Address"/> belongs to.
/// </summary>
public enum AddressFamily
{
    /// <summary>
    /// A 32-bit IPv4 address, an <see cref="Address4"/>.
    /// </summary>
    IPv4,

    /// <summary>
    /// A 128-bit IPv6 address, an <see cref="Address6"/>.
    /// </summary>
    IPv6
}
