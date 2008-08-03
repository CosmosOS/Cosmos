using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;
using Mono.Terminal;

namespace mono_curses.Plugs
{
    [Plug(Target=typeof(Widget))]
    public static class WidgetImpl
    {
        public static void Log(string s) {
            // not implemented, but dont throw an exception..
        }

        public static void Log(string s, params object[] args) {
            // not implemented, but dont throw an exception..
        }

    }
}