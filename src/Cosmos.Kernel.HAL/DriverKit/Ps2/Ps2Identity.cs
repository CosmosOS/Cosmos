// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// The identity of a Ps2 node: the port of the 8042 it hangs off, and
/// nothing else. Paths are <c>ps2:kbd</c> and <c>ps2:aux</c>. Constructed
/// by the 8042 driver in Cosmos.Kernel.Drivers, so the constructor is
/// public, as <see cref="Platform.PlatformIdentity"/>'s is.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class Ps2Identity : DeviceIdentity
{
    /// <summary>Creates the identity of one port.</summary>
    /// <param name="port">The port.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is neither <see cref="Ps2Port.Keyboard"/> nor <see cref="Ps2Port.Auxiliary"/>.</exception>
    public Ps2Identity(Ps2Port port)
    {
        if (port != Ps2Port.Keyboard && port != Ps2Port.Auxiliary)
        {
            throw new ArgumentOutOfRangeException(nameof(port));
        }

        Port = port;
    }

    /// <summary>The port of the 8042 this node hangs off. Any context.</summary>
    public Ps2Port Port { get; }

    /// <inheritdoc/>
    public override string BusName => "ps2";

    /// <inheritdoc/>
    public override string Address => Port == Ps2Port.Keyboard ? "kbd" : "aux";

    /// <inheritdoc/>
    public override string Describe() => Port == Ps2Port.Keyboard ? "port kbd" : "port aux";
}
