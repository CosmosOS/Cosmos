// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.Kernel.System
{
    partial class ExtendedConsole
    {
        private static readonly Dictionary<int, ConsoleColor> s_dict = new()
        {
            { Color.Black.ToArgb(), ConsoleColor.Black },
            { Color.DarkBlue.ToArgb(), ConsoleColor.DarkBlue },
            { Color.DarkGreen.ToArgb(), ConsoleColor.DarkGreen },
            { Color.DarkCyan.ToArgb(), ConsoleColor.DarkCyan },
            { Color.DarkRed.ToArgb(), ConsoleColor.DarkRed },
            { Color.DarkMagenta.ToArgb(), ConsoleColor.DarkMagenta },
            { Color.Olive.ToArgb(), ConsoleColor.DarkYellow },
            { Color.Gray.ToArgb(), ConsoleColor.Gray },
            { Color.DarkGray.ToArgb(), ConsoleColor.DarkGray },
            { Color.Blue.ToArgb(), ConsoleColor.Blue },
            { Color.Green.ToArgb(), ConsoleColor.Green },
            { Color.Cyan.ToArgb(), ConsoleColor.Cyan },
            { Color.Red.ToArgb(), ConsoleColor.Red },
            { Color.Magenta.ToArgb(), ConsoleColor.Magenta },
            { Color.Yellow.ToArgb(), ConsoleColor.Yellow },
            { Color.White.ToArgb(), ConsoleColor.White },
        };
    }
}
