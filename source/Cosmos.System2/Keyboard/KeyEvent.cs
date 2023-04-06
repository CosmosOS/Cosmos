using System;

namespace Cosmos.System
{
    /// <summary>
    /// Represents a key-press event.
    /// </summary>
    public class KeyEvent
    {
        /// <summary>
        /// Represents the type of the a <see cref="KeyEvent"/>.
        /// </summary>
        public enum KeyEventType
        {
            Make,
            Break
        }

        // TODO: As GitHub issue #137 is fixed, this can be replaced with the ConsoleKeyInfo struct.

        /// <summary>
        /// The text character of the key-press event.
        /// </summary>
        public char KeyChar { get; set; }

        /// <summary>
        /// The virtual key of the key-press event.
        /// </summary>
        public ConsoleKeyEx Key { get; set; }

        /// <summary>
        /// The modifiers of the key-press event.
        /// </summary>
        public ConsoleModifiers Modifiers { get; set;}

        /// <summary>
        /// The type of the key-press event.
        /// </summary>
        public KeyEventType Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEvent"/> class.
        /// </summary>
        public KeyEvent()
        {
            KeyChar = '\0';
            Key = ConsoleKeyEx.NoName;
            Modifiers = 0;
            Type = KeyEventType.Make;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyEvent"/> class.
        /// </summary>
        /// <param name="keyChar">The text character.</param>
        /// <param name="key">The virtual key.</param>
        /// <param name="shift">Whether the Shift key was pressed.</param>
        /// <param name="alt">Whether the Alt key was pressed.</param>
        /// <param name="control">Whether the Control (Ctrl) key was pressed.</param>
        /// <param name="type">The type of the <see cref="KeyEvent"/>.</param>
        public KeyEvent(char keyChar, ConsoleKeyEx key, bool shift, bool alt, bool control, KeyEventType type)
        {
            KeyChar = keyChar;
            Key = key;
            Modifiers = (ConsoleModifiers)0;

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
    }
}
