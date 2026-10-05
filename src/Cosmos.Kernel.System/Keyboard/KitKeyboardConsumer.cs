// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// The keyboard manager's consumer of the driver kit: every keyboard a kit
/// driver publishes joins the manager's list and leaves it when withdrawn; a
/// key report goes straight to the manager's scan code handler. Installed by
/// <see cref="KeyboardManager.Initialize"/>, before the driver stage runs.
/// <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run on the kit
/// worker in thread context; <see cref="OnKey"/> runs in the sink caller's
/// context, an interrupt included, and allocates nothing of its own.
/// </summary>
internal sealed class KitKeyboardConsumer : KeyboardConsumer
{
    /// <inheritdoc/>
    public override void OnPublished(PublishedDevice device) => KeyboardManager.RegisterKeyboard(device);

    /// <inheritdoc/>
    public override void OnWithdrawn(PublishedDevice device) => KeyboardManager.UnregisterKeyboard(device);

    /// <inheritdoc/>
    public override void OnKey(PublishedDevice device, byte scanCode, bool released) => KeyboardManager.HandleScanCode(scanCode, released);
}
