// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.System.Mouse;

/// <summary>
/// The ring's view of a pointer a driver kit driver published: a
/// <see cref="MouseDevice"/> over the <see cref="IPointer"/>, the HAL base
/// rather than the bare interface so <see cref="MouseManager"/> hooks its
/// <see cref="MouseDevice.OnMouseEvent"/> as it does for every platform
/// mouse. Created by <see cref="KitPointerConsumer"/> on the kit worker when
/// the pointer is published; reports reach <see cref="OnRelative"/> and
/// <see cref="OnAbsolute"/> through the consumer, in the driver's context.
/// The reads are any context.
/// </summary>
internal sealed class KitPointerDevice : MouseDevice
{
    private readonly PublishedDevice _published;
    private readonly IPointer _pointer;

    /// <summary>Wraps a published pointer. Kit worker, thread context.</summary>
    /// <param name="published">The published device the pointer came with.</param>
    /// <param name="pointer">The driver's pointer contract.</param>
    internal KitPointerDevice(PublishedDevice published, IPointer pointer)
    {
        _published = published;
        _pointer = pointer;
    }

    /// <summary>The published device this adapter stands for, the key the consumer finds it by.</summary>
    public PublishedDevice Published => _published;

    /// <summary>The name the driver gave the pointer.</summary>
    public string Name => _pointer.Name;

    /// <summary>Always false: a kit pointer pushes its reports through the consumer and buffers nothing here.</summary>
    public override bool DataAvailable => false;

    /// <summary>Nothing to do: the kit driver brought the device up in its probe, before publishing it.</summary>
    public override void Initialize()
    {
    }

    /// <summary>Nothing to do: a published pointer is always enabled, and the driver's teardown withdraws it.</summary>
    public override void Enable()
    {
    }

    /// <summary>Nothing to do: see <see cref="Enable"/>.</summary>
    public override void Disable()
    {
    }

    /// <summary>
    /// A relative movement report: moves the device's own position by the
    /// deltas, records the wheel and the buttons, and raises
    /// <see cref="MouseDevice.OnMouseEvent"/> with the deltas, as a PS/2 mouse
    /// does for one packet. Sink caller's context, an interrupt included;
    /// allocation-free.
    /// </summary>
    /// <param name="deltaX">Horizontal movement since the last report.</param>
    /// <param name="deltaY">Vertical movement since the last report.</param>
    /// <param name="buttons">Buttons held down.</param>
    /// <param name="wheel">Wheel movement since the last report.</param>
    internal void OnRelative(int deltaX, int deltaY, PointerButtons buttons, int wheel)
    {
        X += deltaX;
        Y += deltaY;
        ScrollDelta = wheel;
        SetButtons(buttons);
        OnMouseEvent?.Invoke(deltaX, deltaY, wheel, LeftButton, RightButton, MiddleButton);
    }

    /// <summary>
    /// An absolute position report: sets the device's own position and the
    /// buttons, and raises <see cref="MouseDevice.OnMouseEvent"/> with zero
    /// deltas, so the manager learns the buttons and keeps its own cursor
    /// where it was. That is the migration-period mapping of an absolute
    /// device onto a contract that only knows movement. Sink caller's
    /// context, an interrupt included; allocation-free.
    /// </summary>
    /// <param name="x">Horizontal position in the device's own range.</param>
    /// <param name="y">Vertical position in the device's own range.</param>
    /// <param name="buttons">Buttons held down.</param>
    internal void OnAbsolute(int x, int y, PointerButtons buttons)
    {
        X = x;
        Y = y;
        ScrollDelta = 0;
        SetButtons(buttons);
        OnMouseEvent?.Invoke(0, 0, 0, LeftButton, RightButton, MiddleButton);
    }

    /// <summary>Records the button set on the base's three flags. Allocation-free; any context.</summary>
    /// <param name="buttons">Buttons held down.</param>
    private void SetButtons(PointerButtons buttons)
    {
        LeftButton = (buttons & PointerButtons.Left) != 0;
        RightButton = (buttons & PointerButtons.Right) != 0;
        MiddleButton = (buttons & PointerButtons.Middle) != 0;
    }
}
