// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Drawing;
using Cosmos.Kernel.System.Sessions;

namespace Cosmos.Kernel.System
{
    internal static partial class ConsoleExtensions
    {
        extension(Console)
        {
            public static void SetBackgroundColor(Color color)
            {
                ConsoleSession session = SessionManager.RequireCurrent();
                ConsoleColor colorToSet = ConsoleColor.Custom;

                int index = s_palette.IndexOf((uint)color.ToArgb());
                if (index >= 0)
                {
                    colorToSet = (ConsoleColor)index;
                }

                session.BackgroundColor = colorToSet;
                session.Screen.BackgroundColor = (uint)color.ToArgb();
            }

            public static void SetForegroundColor(Color color)
            {
                ConsoleSession session = SessionManager.RequireCurrent();
                ConsoleColor colorToSet = ConsoleColor.Custom;

                int index = s_palette.IndexOf((uint)color.ToArgb());
                if (index >= 0)
                {
                    colorToSet = (ConsoleColor)index;
                }

                session.ForegroundColor = colorToSet;
                session.Screen.ForegroundColor = (uint)color.ToArgb();
            }
        }
    }
}
