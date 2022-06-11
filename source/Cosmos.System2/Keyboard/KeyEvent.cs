using System;

namespace Cosmos.System;

/// <summary>
///     KeyEvent class. Represent key event.
/// </summary>
public class KeyEvent
{
    /// <summary>
    ///     Key event type.
    /// </summary>
    public enum KeyEventType
    {
        Make,
        Break
    }

    /// <summary>
    ///     Create new instance of the <see cref="KeyEvent" /> class.
    /// </summary>
    public KeyEvent()
    {
        KeyChar = '\0';
        Key = ConsoleKeyEx.NoName;
        Modifiers = 0;
        Type = KeyEventType.Make;
    }

    /// <summary>
    ///     Create new instance of the <see cref="KeyEvent" /> class.
    /// </summary>
    /// <param name="keyChar">Key char.</param>
    /// <param name="key">Key.</param>
    /// <param name="shift">Shift.</param>
    /// <param name="alt">Alt.</param>
    /// <param name="control">Ctrl.</param>
    /// <param name="type">Type.</param>
    public KeyEvent(char keyChar, ConsoleKeyEx key, bool shift, bool alt, bool control, KeyEventType type)
    {
        KeyChar = keyChar;
        Key = key;
        Modifiers = 0;
        if (shift)
        {
            Modifiers |= ConsoleModifiers.Shift;
        }

        if (alt)
        {
            Modifiers |= ConsoleModifiers.Alt;
        }

        if (control)
        {
            Modifiers |= ConsoleModifiers.Control;
        }

        Type = type;
    }

    // todo: once Github issue #137 is fixed, replace this class with ConsoleKeyInfo struct.
    // Well, this one has more features

    /// <summary>
    ///     Get and set key char.
    /// </summary>
    public char KeyChar { get; set; }

    /// <summary>
    ///     Get and set key.
    /// </summary>
    public ConsoleKeyEx Key { get; set; }

    /// <summary>
    ///     Get and set console modifiers.
    /// </summary>
    public ConsoleModifiers Modifiers { get; set; }

    /// <summary>
    ///     Get and set key event type.
    /// </summary>
    public KeyEventType Type { get; set; }
}
