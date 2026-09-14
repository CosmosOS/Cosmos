// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// Abstract base class for all keyboard devices.
/// </summary>
internal abstract class KeyboardDevice : Device, IKeyboardDevice
{
    /// <summary>
    /// Event handler invoked when a key is pressed or released.
    /// </summary>
    public KeyPressedHandler? OnKeyPressed { get; set; }

    /// <summary>
    /// Initialize the keyboard device.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Check if a key is available in the buffer.
    /// </summary>
    public abstract bool KeyAvailable { get; }

    /// <summary>
    /// Enable keyboard scanning.
    /// </summary>
    public abstract void Enable();

    /// <summary>
    /// Disable keyboard scanning.
    /// </summary>
    public abstract void Disable();

    /// <summary>
    /// Update keyboard LEDs (Caps Lock, Num Lock, Scroll Lock).
    /// </summary>
    public abstract void UpdateLeds();

    /// <summary>
    /// Poll for keyboard events (default: no-op for interrupt-driven keyboards).
    /// </summary>
    public virtual void Poll()
    {
        // Default implementation does nothing - interrupt-driven keyboards don't need polling
    }

    /// <summary>
    /// Wait for a key to be pressed by halting the CPU.
    /// </summary>
    public static void WaitForKey()
    {
        PlatformHAL.CpuOps?.Halt();
    }
}
