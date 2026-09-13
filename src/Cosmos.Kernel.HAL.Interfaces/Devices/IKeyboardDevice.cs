// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Delegate for handling key press events.
/// </summary>
/// <param name="scanCode">The scan code of the key.</param>
/// <param name="released">True if the key was released, false if pressed.</param>
internal delegate void KeyPressedHandler(byte scanCode, bool released);

/// <summary>
/// Interface for keyboard devices.
/// </summary>
internal interface IKeyboardDevice
{
    /// <summary>
    /// Initialize the keyboard device.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Check if a key is available in the buffer.
    /// </summary>
    bool KeyAvailable { get; }

    /// <summary>
    /// Enable keyboard scanning.
    /// </summary>
    void Enable();

    /// <summary>
    /// Disable keyboard scanning.
    /// </summary>
    void Disable();

    /// <summary>
    /// Update keyboard LEDs (Caps Lock, Num Lock, Scroll Lock).
    /// </summary>
    void UpdateLeds();

    /// <summary>
    /// Poll for keyboard events (for devices that don't use interrupts reliably).
    /// </summary>
    void Poll();

    /// <summary>
    /// Event handler for key press/release events.
    /// </summary>
    KeyPressedHandler? OnKeyPressed { get; set; }
}
