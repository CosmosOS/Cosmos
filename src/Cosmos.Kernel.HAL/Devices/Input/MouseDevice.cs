// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// Delegate for handling mouse events.
/// </summary>
/// <param name="deltaX">Change in X position.</param>
/// <param name="deltaY">Change in Y position.</param>
/// <param name="deltaZ">Wheel movement since the last event: negative scrolls up, positive scrolls down, zero when the device has no wheel or it did not move.</param>
/// <param name="leftButton">Left button state.</param>
/// <param name="rightButton">Right button state.</param>
/// <param name="middleButton">Middle button state.</param>
internal delegate void MouseEventHandler(int deltaX, int deltaY, int deltaZ, bool leftButton, bool rightButton, bool middleButton);

/// <summary>
/// Abstract base class for all mouse devices.
/// </summary>
internal abstract class MouseDevice : Device, IMouseDevice
{
    /// <summary>
    /// Event handler invoked when mouse state changes.
    /// </summary>
    public MouseEventHandler? OnMouseEvent { get; set; }

    /// <summary>
    /// Current X position.
    /// </summary>
    public int X { get; protected set; }

    /// <summary>
    /// Current Y position.
    /// </summary>
    public int Y { get; protected set; }

    /// <summary>
    /// Scroll wheel delta.
    /// </summary>
    public int ScrollDelta { get; protected set; }

    /// <summary>
    /// Left button state.
    /// </summary>
    public bool LeftButton { get; protected set; }

    /// <summary>
    /// Right button state.
    /// </summary>
    public bool RightButton { get; protected set; }

    /// <summary>
    /// Middle button state.
    /// </summary>
    public bool MiddleButton { get; protected set; }

    /// <summary>
    /// Initialize the mouse device.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Check if mouse data is available.
    /// </summary>
    public abstract bool DataAvailable { get; }

    /// <summary>
    /// Enable the mouse.
    /// </summary>
    public abstract void Enable();

    /// <summary>
    /// Disable the mouse.
    /// </summary>
    public abstract void Disable();

    /// <summary>
    /// Poll for mouse events (default: no-op for interrupt-driven mice).
    /// </summary>
    public virtual void Poll()
    {
        // Default implementation does nothing - interrupt-driven mice don't need polling
    }
}
