// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System
{
    internal static partial class ExtendedConsole
    {
        public static void SetBackgroundColor(Color color)
        {
            ConsoleSession session = SessionManager.RequireCurrent();

            if (s_dict.TryGetValue(color.ToArgb(), out ConsoleColor conColor))
            {
                Console.BackgroundColor = conColor;
                return;
            }

            session.BackgroundColor = ConsoleColor.Custom;
            session.Screen.SetBackgroundColor(color);
            return;
        }

        public static void SetForegroundColor(Color color)
        {
            ConsoleSession session = SessionManager.RequireCurrent();

            if (s_dict.TryGetValue(color.ToArgb(), out ConsoleColor conColor))
            {
                Console.ForegroundColor = conColor;
                return;
            }

            session.ForegroundColor = ConsoleColor.Custom;
            session.Screen.SetForegroundColor(color);
            return;
        }
    }
}
