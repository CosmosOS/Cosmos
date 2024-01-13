using System;

namespace Cosmos.System.Keyboard
{
    public static class KeyEventHelper
    {
        internal static ConsoleKeyInfo ToConsoleKeyInfo(this KeyEvent keyEvent)
        {
            //TODO: Plug HasFlag and use the next 3 lines instead of the 3 following lines

            //bool xShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            //bool xAlt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            //bool xControl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

            bool xShift = (keyEvent.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
            bool xAlt = (keyEvent.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt;
            bool xControl = (keyEvent.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

            return new ConsoleKeyInfo(keyEvent.KeyChar, keyEvent.Key.ToConsoleKey(), xShift, xAlt, xControl);
        }
    }
}