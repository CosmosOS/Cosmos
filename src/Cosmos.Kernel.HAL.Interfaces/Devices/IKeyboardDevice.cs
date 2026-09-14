// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// Delegate for handling key press events.
/// </summary>
/// <param name="scanCode">The scan code set 1 make code of the key, with the
/// 0xE0 prefix of an extended key dropped, except for the right Alt key, which
/// arrives as <see cref="IKeyboardDevice.RightAltScanCode"/>.</param>
/// <param name="released">True if the key was released, false if pressed.</param>
internal delegate void KeyPressedHandler(byte scanCode, bool released);

/// <summary>
/// Interface for keyboard devices.
/// </summary>
internal interface IKeyboardDevice
{
    /// <summary>
    /// Scan code every keyboard device reports for the right Alt key. On the
    /// wire it is the extended form of the left Alt (E0 38), and dropping the
    /// prefix like every other extended key would make the two Alt keys the
    /// same key; a layout maps this code to <c>ConsoleKeyEx.AltGr</c> or
    /// <c>ConsoleKeyEx.RAlt</c> to say which it is. Set 1 assigns nothing to
    /// 0x60.
    /// </summary>
    const byte RightAltScanCode = 0x60;

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
