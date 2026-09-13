// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Interface for mouse devices.
/// </summary>
internal interface IMouseDevice
{
    /// <summary>
    /// Initialize the mouse device.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Check if mouse data is available.
    /// </summary>
    bool DataAvailable { get; }

    /// <summary>
    /// Current X position.
    /// </summary>
    int X { get; }

    /// <summary>
    /// Current Y position.
    /// </summary>
    int Y { get; }

    /// <summary>
    /// Scroll wheel delta.
    /// </summary>
    int ScrollDelta { get; }

    /// <summary>
    /// Left button state.
    /// </summary>
    bool LeftButton { get; }

    /// <summary>
    /// Right button state.
    /// </summary>
    bool RightButton { get; }

    /// <summary>
    /// Middle button state.
    /// </summary>
    bool MiddleButton { get; }

    /// <summary>
    /// Enable the mouse.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disable the mouse.
    /// </summary>
    void Disable();

    /// <summary>
    /// Poll for mouse events (for devices that don't use interrupts reliably).
    /// </summary>
    void Poll();
}
