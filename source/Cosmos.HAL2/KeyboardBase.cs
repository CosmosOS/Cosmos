using Cosmos.Core;

namespace Cosmos.HAL;

public abstract class KeyboardBase : Device
{
    public delegate void KeyPressedEventHandler(byte ScanCode, bool Released);

    public KeyPressedEventHandler OnKeyPressed;

    /// <summary>
    ///     Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    ///     Update keyboard LEDs.
    /// </summary>
    public abstract void UpdateLeds();

    /// <summary>
    ///     Wait for key to be pressed.
    /// </summary>
    public static void WaitForKey() => CPU.Halt();
}
