using System;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System
{
    [Plug(TargetName = "System.ConsolePal, System.Console")]
    public class ConsolePalImpl
    {
        // ReadKey() pure CIL

        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            var key = KeyboardManager.ReadKey();
            if (intercept == false && key.KeyChar != '\0')
            {
                global::System.Console.Write(key.KeyChar);
            }

            return new ConsoleKeyInfo(key.KeyChar, key.Key.ToConsoleKey(), (key.Modifiers & ConsoleModifiers.Shift) != 0, (key.Modifiers & ConsoleModifiers.Alt) != 0, (key.Modifiers & ConsoleModifiers.Control) != 0);
        }
    }
}
