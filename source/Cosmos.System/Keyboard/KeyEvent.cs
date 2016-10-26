using System;

namespace Cosmos.System
{
    public class KeyEvent
    {
        public enum KeyEventType
        {
            Make,
            Break
        }

        // todo: once Github issue #137 is fixed, replace this class with ConsoleKeyInfo struct.
        // Well, this one has more features

        public char KeyChar
        {
            get;
            set;
        }

        public ConsoleKeyEx Key
        {
            get;
            set;
        }

        public ConsoleModifiers Modifiers
        {
            get;
            set;
        }

        public KeyEventType Type { get; set; }

        public KeyEvent()
        {
            KeyChar = '\0';
            Key = ConsoleKeyEx.NoName;
            this.Modifiers = (ConsoleModifiers)0;
            Type = KeyEventType.Make;
        }

        public KeyEvent(char keyChar, ConsoleKeyEx key, bool shift, bool alt, bool control, KeyEventType type)
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Modifiers = (ConsoleModifiers)0;
            if (shift)
            {
                this.Modifiers |= ConsoleModifiers.Shift;
            }
            if (alt)
            {
                this.Modifiers |= ConsoleModifiers.Alt;
            }
            if (control)
            {
                this.Modifiers |= ConsoleModifiers.Control;
            }
            this.Type = type;
        }
    }
}
