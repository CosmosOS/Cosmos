// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.HAL.DriverKit.Platform;

/// <summary>
/// What a machine description supplies so the kit can route a line of the
/// platform's interrupt controller without naming the controller: the
/// ARM64 description hands the GIC's routing, another platform its own.
/// A <see cref="PlatformLineInterruptSource"/> forwards every connect,
/// disconnect, mask and unmask to the routing it was created with, so the
/// kit's source stays free of the controller's registers and the arch HAL
/// keeps them. Lines are the controller's own ids (an INTID on ARM64).
/// </summary>
internal abstract class PlatformLineRouting
{
    /// <summary>
    /// Installs <paramref name="handler"/> on <paramref name="line"/> and
    /// enables the line at the controller, the handler first so a level
    /// line already asserted fires into it on enable. Thread context.
    /// </summary>
    /// <param name="line">The controller's id of the line.</param>
    /// <param name="handler">The platform-shaped handler to run on every delivery.</param>
    /// <returns>False when the line is taken by another handler or the controller cannot route it.</returns>
    public abstract bool TryConnect(uint line, InterruptManager.IrqDelegate handler);

    /// <summary>
    /// Undoes <see cref="TryConnect"/>: disables the line at the controller
    /// and clears its handler, so a delivery already latched finds no
    /// handler. Thread context.
    /// </summary>
    /// <param name="line">The controller's id of the line.</param>
    public abstract void Disconnect(uint line);

    /// <summary>Stops deliveries of the line at the controller until <see cref="Unmask"/>. Allocation-free; any context.</summary>
    /// <param name="line">The controller's id of the line.</param>
    public abstract void Mask(uint line);

    /// <summary>Lets deliveries of the line through again. Allocation-free; any context.</summary>
    /// <param name="line">The controller's id of the line.</param>
    public abstract void Unmask(uint line);
}
