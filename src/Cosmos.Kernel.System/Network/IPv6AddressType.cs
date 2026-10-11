// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// The scope or role an IPv6 address has by its prefix, as <see cref="Address6.AddressType"/> reads it.
/// </summary>
public enum IPv6AddressType
{
    /// <summary>
    /// An address assigned to several interfaces, delivered to the nearest one.
    /// </summary>
    Anycast,

    /// <summary>
    /// A routable address in <c>2000::/3</c>.
    /// </summary>
    GlobalUnicast,

    /// <summary>
    /// A link-local address in <c>fe80::/10</c>.
    /// </summary>
    LinkLocal,

    /// <summary>
    /// The loopback address <c>::1</c>.
    /// </summary>
    Loopback,

    /// <summary>
    /// The unspecified address <c>::</c>.
    /// </summary>
    Unspecified,

    /// <summary>
    /// A unique local address in <c>fc00::/7</c>.
    /// </summary>
    UniqueLocal,

    /// <summary>
    /// An IPv4 address carried in the low 32 bits, as in <c>::ffff:192.0.2.128</c>.
    /// </summary>
    EmbeddedIPv4,

    /// <summary>
    /// A well-known multicast address in <c>ff00::/12</c>.
    /// </summary>
    WellKnown,

    /// <summary>
    /// A transient multicast address.
    /// </summary>
    Transient,

    /// <summary>
    /// A solicited-node multicast address in <c>ff02::1:ff00:0/104</c>.
    /// </summary>
    SolicitedNode,

    /// <summary>
    /// Any address no other value describes.
    /// </summary>
    Generic,
}
