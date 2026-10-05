// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Ps2;

/// <summary>
/// The contract an 8042 controller driver implements for the kit: how a
/// byte is sent to a port, how the status register is polled once, and
/// whether bytes arrive on their own (interrupts or a periodic drain) or
/// only while a probe polls. The controller driver binds the 8042's
/// platform node, owns its two ports and its two lines, and creates one
/// <see cref="Ps2Access"/> over this object per port; the kit never reaches
/// a port or an interrupt controller for this bus kind. A controller driver
/// implements the protected members; the kit calls them through the
/// internal ones, so a leaf driver holding a reference can reach neither.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class Ps2Controller
{
    /// <summary>Creates the contract half of a controller.</summary>
    protected Ps2Controller()
    {
    }

    /// <summary>
    /// True once the controller's lines are connected, so a byte reaches
    /// <see cref="Ps2Access.Deliver"/> from the controller driver's
    /// interrupt handler without anyone polling. Any context.
    /// </summary>
    public abstract bool InterruptDriven { get; }

    /// <summary>
    /// True when the controller driver drains the status register from a
    /// periodic work item on the kit worker because a line could not be
    /// routed. Any context.
    /// </summary>
    public abstract bool PolledPeriodically { get; }

    /// <summary>
    /// Writes one byte to the port's device: the 0xD4 prefix for the
    /// auxiliary port, then the byte, each after the controller's input
    /// buffer emptied. Thread context.
    /// </summary>
    /// <param name="port">The port whose device receives the byte.</param>
    /// <param name="value">The byte.</param>
    /// <returns>False when the input buffer did not empty within the controller driver's bound.</returns>
    protected abstract bool TrySendCore(Ps2Port port, byte value);

    /// <summary>
    /// Reads the status register once and, while the output buffer is full,
    /// hands each byte to the <see cref="Ps2Access.Deliver"/> of the port
    /// the status attributes it to. Thread context, from an exchange that
    /// waits on a controller that does not deliver on its own.
    /// </summary>
    protected abstract void PollCore();

    /// <summary>Kit side of <see cref="TrySendCore"/>; called by the access's exchange, never by a leaf driver. Thread context.</summary>
    /// <param name="port">The port whose device receives the byte.</param>
    /// <param name="value">The byte.</param>
    /// <returns>False when the input buffer did not empty within the controller driver's bound.</returns>
    internal bool TrySend(Ps2Port port, byte value) => TrySendCore(port, value);

    /// <summary>Kit side of <see cref="PollCore"/>; called by the access's exchange, never by a leaf driver. Thread context.</summary>
    internal void Poll() => PollCore();
}
