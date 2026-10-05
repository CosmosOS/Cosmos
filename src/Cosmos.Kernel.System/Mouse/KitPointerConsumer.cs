// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Mouse;

/// <summary>
/// The mouse manager's consumer of the driver kit: every pointer a kit driver
/// publishes joins the manager's list and leaves it when withdrawn; a
/// movement or button report goes straight to the manager's handlers.
/// Installed by <see cref="MouseManager.Initialize"/>, before the driver
/// stage runs. <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run on
/// the kit worker in thread context; <see cref="OnRelative"/> and
/// <see cref="OnAbsolute"/> run in the sink caller's context, an interrupt
/// included, and allocate nothing of their own.
/// </summary>
internal sealed class KitPointerConsumer : PointerConsumer
{
    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device) => MouseManager.RegisterMouse(device);

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device) => MouseManager.UnregisterMouse(device);

    /// <inheritdoc/>
    public override void OnRelative(PublishedDevice device, int deltaX, int deltaY, PointerButtons buttons, int wheel) =>
        MouseManager.HandleRelative(deltaX, deltaY, buttons, wheel);

    /// <inheritdoc/>
    public override void OnAbsolute(PublishedDevice device, int x, int y, PointerButtons buttons) =>
        MouseManager.HandleAbsolute(x, y, buttons);
}
