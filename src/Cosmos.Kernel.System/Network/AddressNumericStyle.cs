// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The number base an IPv4 address is parsed from or formatted in.
/// </summary>
public enum AddressNumericStyle
{
    /// <summary>
    /// Decimal octets, as in <c>192.168.1.1</c>.
    /// </summary>
    Dec,

    /// <summary>
    /// Hexadecimal octets, as in <c>c0.a8.1.1</c>.
    /// </summary>
    Hex
}
