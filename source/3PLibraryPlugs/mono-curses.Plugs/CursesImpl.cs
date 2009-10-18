using System;
using System.Linq;
using Cosmos.IL2CPU.Plugs;
using Mono.Terminal;

namespace mono_curses.Plugs {
    [Plug(Target = typeof(Curses))]
    public static class CursesImpl {
        private const int MaxColors = 256;
        private static readonly int[] mColors = new int[256]; // 256 colors
        private static bool mEcho = true;

        public static bool has_colors() {
            return true;
        }

        public static int init_pair(short pair,
                                    short f,
                                    short b) {
            mColors[pair - 1] = f + (b << 16);
            return 0;
        }

        public static int start_color() {
            return 0;
        }

        public static long console_sharp_mouse_mask(long newmask,
                                                    out long oldmask) {
            oldmask = -1;
            return -1;
        }

        public static IntPtr console_sharp_get_curscr() {
            return IntPtr.Zero;
        }

        public static IntPtr console_sharp_get_stdscr() {
            return IntPtr.Zero;
        }

        public static IntPtr real_initscr() {
            //Console.Clear();
            return IntPtr.Zero;
        }

        public static void console_sharp_get_dims(out int lines,
                                                  out int cols) {
            lines = Console.WindowHeight;
            cols = Console.WindowWidth;
        }

        private static char ConvertChar(int aChar) {
            switch (aChar) {
                case 0x40006c:
                    // upper left-hand corner
                    return '+';
                case 0x400071:
                // horizontal line
                    return '-';
                case 0x40006b:
                // upper right-hand corner
                    return '+';
                case 0x400078:
                    // vertical line
                    return '|';
                case 0x40006d:
                    // lower left-hand corner
                    return '+';
                case 0x40006a:
                    // lower right-hand corner
                    return '+';

                default: {
                    if ((aChar & 0x400000) != 0) {
                        Console.Write("Special char not handled: ");
                        Console.WriteLine(aChar.ToString());
                        return ' ';
                    }
                    return (char)aChar;
                }
            }
        }

        public static int addch(int aChar) {
            Console.Write(ConvertChar(aChar));
            return 0;
        }

        public static int addstr(string aStr) {
            for (int i = 0; i < aStr.Length; i++) {
                addch(aStr[i]);
            }
            return 0;
        }

        public static int attrset(int aAttributes) {
            // todo: implement properly
            return 0;
        }

        public static int endwin() {
            return 0;
        }

        public static int getch() {
            var xChar = (char)Console.Read();
            if(mEcho){Console.Write(xChar);}
            return (int)xChar;
        }

        public static int keypad(IntPtr win, bool bf) {
            return 0;
        }

        public static int move(int x, int y) {
            Console.CursorLeft = x;
            Console.CursorTop = y;
            return 0;
        }
        public static int noecho(){
            mEcho = false;
            return 0;
        }
        public static int raw() {
            // todo: implement properly
            return 0;
        }

        public static int redrawwin(IntPtr aWin) {
            // todo: draw window
            Console.WriteLine("RedrawWin");
            return 0;
        }

        public static int refresh() {
            // todo: draw stuff
            Console.WriteLine("Refresh");
            return 0;
        }

        public static int timeout(int aTimeout) {
            // todo: implement timeout
            return 0;
        }

        public static uint console_sharp_getmouse(out Curses.MouseEvent ev) {
            ev = default(Curses.MouseEvent);
            return 0;
        }

        public static void console_sharp_sendsigtstp() {
            // todo: property implement
        }

    }
}