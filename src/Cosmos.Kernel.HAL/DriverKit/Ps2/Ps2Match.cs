// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// A match over Ps2 identities: one port, specificity 1. There is no match
/// for any port: the bus has two ports of different kinds, and a driver
/// matching both would be offered a mouse's port as a keyboard.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class Ps2Match : DeviceMatch
{
    private readonly Ps2Port _port;

    private Ps2Match(Ps2Port port)
    {
        _port = port;
    }

    /// <inheritdoc/>
    public override int Specificity => 1;

    /// <summary>Matches the Ps2 node of <paramref name="port"/>.</summary>
    /// <param name="port">The port.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is neither <see cref="Ps2Port.Keyboard"/> nor <see cref="Ps2Port.Auxiliary"/>.</exception>
    public static Ps2Match Port(Ps2Port port)
    {
        if (port != Ps2Port.Keyboard && port != Ps2Port.Auxiliary)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        return new Ps2Match(port);
    }

    /// <inheritdoc/>
    public override bool Matches(DeviceIdentity identity) => identity is Ps2Identity ps2 && ps2.Port == _port;
}
