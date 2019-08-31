using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZLibrary.Constants;

namespace ZLibrary.Machine
{
    public static class WindowType
    {
        public const int Main = 0;
        public const int Status = 1;
    }

    public class VirtualConsole
    {
        public int Top { get; private set; }
        public int Left { get; private set; }
        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public int CursorRow { get; set; }
        public int CursorColumn { get; set; }
        private char[][] _buffer;
        private int _savedX;
        private int _savedY;

        public VirtualConsole(int aRows, int aColumns, int aTop, int aLeft)
        {
            Rows = aRows;
            Columns = aColumns;
            Top = aTop;
            Left = aLeft;

            _buffer = new char[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                _buffer[i]= new char[Columns];
            }

            CursorRow = Top;
            CursorColumn = Left;
        }

        public void SendToScreen()
        {
            Console.CursorTop = Top;
            Console.CursorLeft = Left;
            foreach (var line in _buffer)
            {
                Console.WriteLine(line);
            }
        }

        public void Write(char c)
        {
            if (CursorColumn >= Columns || c == '\n')
            {
                CursorRow++;
                CursorColumn = Left;
            }

            if (CursorRow >= Rows)
            {
                if (Rows > 1)
                {
                    ScrollScreen();
                }
                CursorRow--;
            }

            if (c == '\n')
            {
                return;
            }

            _buffer[CursorRow][CursorColumn] = c;
            CursorColumn++;
        }

        public void Write(string s)
        {
            foreach (char c in s)
            {
                Write(c);
            }
        }

        public void SaveCursorPosition()
        {
            _savedX = CursorColumn;
            _savedY = CursorRow;
        }

        public void RestoreCursorPosition()
        {
            int x = _savedX + Left;
            int y = _savedY + Top;
            Console.SetCursorPosition(x, y);
        }

        private void ScrollScreen()
        {
            for (int i = 0; i < Rows - 1; i++)
            {
                _buffer[i] = _buffer[i + 1];
            }
            _buffer[Rows - 1] = new char[Columns];
        }
    }

    public class ZConsoleScreen : IZScreen
    {
        private int _curentWindow;
        private readonly ConsoleColor bgupper = ConsoleColor.Black;
        private readonly ConsoleColor fgupper = ConsoleColor.Gray;
        private readonly ConsoleColor bglower = ConsoleColor.Black;
        private readonly ConsoleColor fglower = ConsoleColor.Gray;
        private bool reverse, emphasis;

        private int lineCount;

        private const int MAX_COMMAND_HISTORY = 10;
        private readonly List<string> history = new List<string>();

        private readonly ZMachine _machine;
        private List<VirtualConsole> _consoles = new List<VirtualConsole>();

        public ZConsoleScreen(ZMachine aMachine)
        {
            _machine = aMachine;
            _consoles.Add(new VirtualConsole(19, 70, 1, 0));
            _consoles.Add(new VirtualConsole(1, 70, 0, 0));
        }

        public string ReadLine(string aInitialValue, int aTimeout, ushort timeoutRoutine, byte[] terminatingKeys, out byte terminator)
        {
            lineCount = 0;

            int histIdx = history.Count;
            string savedEntry = string.Empty;
            int sleeps = 0;

            StringBuilder sb;
            int cursor;
            if (aInitialValue.Length == 0)
            {
                sb = new StringBuilder(20);
                cursor = 0;
            }
            else
            {
                sb = new StringBuilder(aInitialValue);
                cursor = aInitialValue.Length;
            }

            while (true)
            {
                if (aTimeout > 0)
                {
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                        if (Console.KeyAvailable)
                        {
                            break;
                        }

                        sleeps++;
                        if (sleeps == aTimeout)
                        {
                            sleeps = 0;
                            int cx = Console.CursorLeft;
                            int cy = Console.CursorTop;
                            if (_machine.Input.HandleInputTimer(timeoutRoutine))
                            {
                                terminator = 0;
                                return string.Empty;
                            }

                            if (Console.CursorLeft != cx || Console.CursorTop != cy)
                            {
                                _consoles[_curentWindow].Write(sb.ToString());
                                for (var i = cursor; i < sb.Length; i++)
                                {
                                    _consoles[_curentWindow].Write('\x08');
                                }
                            }
                        }
                    }
                }

                ConsoleKeyInfo info = Console.ReadKey(true);
                var special = ConsoleKeyToZSCII(info.Key);
                if (IsTerminator(special, terminatingKeys))
                {
                    terminator = special;
                    break;
                }

                switch (info.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (cursor > 0)
                        {
                            cursor--;
                            _consoles[_curentWindow].Write('\x08');
                        }

                        break;

                    case ConsoleKey.RightArrow:
                        if (cursor < sb.Length)
                        {
                            _consoles[_curentWindow].Write(sb[cursor]);
                            cursor++;
                        }

                        break;

                    case ConsoleKey.Home:
                        while (cursor > 0)
                        {
                            cursor--;
                            _consoles[_curentWindow].Write('\x08');
                        }

                        break;

                    case ConsoleKey.End:
                        while (cursor < sb.Length)
                        {
                            _consoles[_curentWindow].Write(sb[cursor]);
                            cursor++;
                        }

                        break;

                    case ConsoleKey.UpArrow:
                        if (histIdx > 0 && history.Count > 0)
                        {
                            if (histIdx == history.Count)
                            {
                                savedEntry = sb.ToString();
                            }

                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write(' ');
                            }

                            for (var i = 0; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write("\x08 \x08");
                            }

                            histIdx--;
                            sb.Length = 0;
                            sb.Append(history[histIdx]);
                            _consoles[_curentWindow].Write(sb.ToString());
                            cursor = sb.Length;
                        }

                        break;

                    case ConsoleKey.DownArrow:
                        if (histIdx < history.Count && history.Count > 0)
                        {
                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write(' ');
                            }

                            for (var i = 0; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write("\x08 \x08");
                            }

                            histIdx++;
                            sb.Length = 0;
                            if (histIdx == history.Count)
                            {
                                sb.Append(savedEntry);
                            }
                            else
                            {
                                sb.Append(history[histIdx]);
                            }

                            _consoles[_curentWindow].Write(sb.ToString());
                            cursor = sb.Length;
                        }

                        break;

                    case ConsoleKey.Backspace:
                        if (cursor > 0)
                        {
                            cursor--;
                            sb.Remove(cursor, 1);
                            _consoles[_curentWindow].Write('\x08');
                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write(sb[i]);
                            }

                            _consoles[_curentWindow].Write(' ');
                            for (var i = cursor; i <= sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write('\x08');
                            }
                        }

                        break;

                    case ConsoleKey.Delete:
                        if (cursor < sb.Length)
                        {
                            sb.Remove(cursor, 1);
                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write(sb[i]);
                            }

                            _consoles[_curentWindow].Write(' ');
                            for (var i = cursor; i <= sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write('\x08');
                            }
                        }

                        break;

                    case ConsoleKey.Escape:
                        for (var i = cursor; i < sb.Length; i++)
                        {
                            _consoles[_curentWindow].Write(' ');
                        }

                        for (var i = 0; i < sb.Length; i++)
                        {
                            _consoles[_curentWindow].Write("\x08 \x08");
                        }

                        sb.Length = 0;
                        break;

                    default:
                        if (info.KeyChar != '\0')
                        {
                            sb.Insert(cursor, info.KeyChar);
                            _consoles[_curentWindow].Write(info.KeyChar);
                            Console.Write(info.KeyChar);
                            cursor++;
                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write(sb[i]);
                            }

                            for (var i = cursor; i < sb.Length; i++)
                            {
                                _consoles[_curentWindow].Write('\x08');
                            }
                        }

                        break;
                }
            }

            if (terminator == 13)
            {
                _consoles[_curentWindow].Write("\n");
            }

            var result = sb.ToString();

            history.Add(result);
            if (history.Count > MAX_COMMAND_HISTORY)
            {
                history.RemoveAt(0);
            }

            return result;
        }

        public short ReadKey(int time, ushort timeoutCallback)
        {
            lineCount = 0;

            while (true)
            {
                if (time > 0)
                {
                    var sleeps = 0;
                    while (!Console.KeyAvailable)
                    {
                        Thread.Sleep(100);
                        if (Console.KeyAvailable)
                        {
                            break;
                        }

                        sleeps++;
                        if (sleeps == time)
                        {
                            sleeps = 0;
                            if (_machine.Input.HandleInputTimer(timeoutCallback))
                            {
                                return 0;
                            }
                        }
                    }
                }

                ConsoleKeyInfo info = Console.ReadKey(true);
                short zkey = ConsoleKeyToZSCII(info.Key);
                if (zkey != 0)
                {
                    return zkey;
                }

                zkey = _machine.Input.FilterInput(ZText.CharToZSCII(info.KeyChar));
                if (zkey != 0)
                {
                    return zkey;
                }
            }
        }

        public void WriteChar(char ch)
        {
            _consoles[_curentWindow].Write(ch);
            CheckMore();
        }

        public void WriteString(string str)
        {
            foreach (char ch in str)
            {
                WriteChar(ch);
            }
            _consoles[_curentWindow].SendToScreen();
        }

        public void ShowStatus()
        {
            if (_machine.Story.Header.Version > FileVersion.V3)
            {
                return;
            }

            _machine.Memory.GetWord(_machine.Story.Header.GlobalsOffset, out var locationAddr);
            string locationStr = _machine.GetObjectName(locationAddr);
            _machine.Memory.GetWord(_machine.Story.Header.GlobalsOffset + 2, out var hoursOrScore);
            _machine.Memory.GetWord(_machine.Story.Header.GlobalsOffset + 4, out var minsOrTurns);

            bool useTime;
            if (_machine.Story.Header.Version < FileVersion.V3)
            {
                useTime = false;
            }
            else
            {
                _machine.Memory.GetByte(0x1, out var flags1);
                useTime = ((flags1 & 2) != 0);
            }

            SelectWindow(1);
            MoveCursor(1, 1);
            SetTextStyle(FontStyle.REVERSE_STYLE);

            bool brief = (byte)Console.WindowWidth < 55;

            WriteChar(' ');
            WriteString(locationStr);

            if (useTime)
            {
                PadStatusLine(brief ? 15 : 20);

                WriteString("Time: ");

                int hours = (hoursOrScore + 11) % 12 + 1;
                if (hours < 10)
                {
                    WriteChar(' ');
                }

                WriteString(hours.ToString());

                WriteChar(':');

                if (minsOrTurns < 10)
                {
                    WriteChar('0');
                }

                WriteString(minsOrTurns.ToString());

                WriteString(hoursOrScore >= 12 ? " pm" : " am");
            }
            else
            {
                PadStatusLine(brief ? 15 : 30);

                WriteString(brief ? "S: " : "Score: ");
                WriteString(hoursOrScore.ToString());

                PadStatusLine(brief ? 8 : 14);

                WriteString(brief ? "M: " : "Moves: ");
                WriteString(minsOrTurns.ToString());
            }

            PadStatusLine(0);

            SetTextStyle(FontStyle.NORMAL_STYLE);
            SelectWindow(0);
        }

        private static bool IsTerminator(byte key, byte[] terminatingKeys)
        {
            if (key == 13)
            {
                return true;
            }

            if (terminatingKeys.Length == 0)
            {
                return false;
            }

            if (terminatingKeys[0] == 255)
            {
                return ((key >= 129 && key <= 154) || (key >= 252 && key <= 254));
            }

            return Array.IndexOf(terminatingKeys, key) >= 0;
        }

        private static byte ConsoleKeyToZSCII(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.Delete: return 8;
                case ConsoleKey.Enter: return 13;
                case ConsoleKey.Escape: return 27;
                case ConsoleKey.UpArrow: return 129;
                case ConsoleKey.DownArrow: return 130;
                case ConsoleKey.LeftArrow: return 131;
                case ConsoleKey.RightArrow: return 132;
                case ConsoleKey.F1: return 133;
                case ConsoleKey.F2: return 134;
                case ConsoleKey.F3: return 135;
                case ConsoleKey.F4: return 136;
                case ConsoleKey.F5: return 137;
                case ConsoleKey.F6: return 138;
                case ConsoleKey.F7: return 139;
                case ConsoleKey.F8: return 140;
                case ConsoleKey.F9: return 141;
                case ConsoleKey.F10: return 142;
                case ConsoleKey.F11: return 143;
                case ConsoleKey.F12: return 144;
                case ConsoleKey.NumPad0: return 145;
                case ConsoleKey.NumPad1: return 146;
                case ConsoleKey.NumPad2: return 147;
                case ConsoleKey.NumPad3: return 148;
                case ConsoleKey.NumPad4: return 149;
                case ConsoleKey.NumPad5: return 150;
                case ConsoleKey.NumPad6: return 151;
                case ConsoleKey.NumPad7: return 152;
                case ConsoleKey.NumPad8: return 153;
                case ConsoleKey.NumPad9: return 154;
                default: return 0;
            }
        }

        private void GetConsoleColors(out ConsoleColor fg, out ConsoleColor bg)
        {
            if (_curentWindow == WindowType.Status)
            {
                bg = bgupper;
                fg = fgupper;
            }
            else
            {
                bg = bglower;
                fg = fglower;
            }

            if (emphasis)
            {
                fg = EmphasizeColor(fg);
            }

            if (reverse)
            {
                ConsoleColor temp = bg;
                bg = fg;
                fg = temp;
            }
        }

        private void SetConsoleColors()
        {
            GetConsoleColors(out var fg, out var bg);
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private void SetTextStyle(ushort style)
        {
            switch (style)
            {
                case FontStyle.NORMAL_STYLE:
                    reverse = false;
                    emphasis = false;
                    break;

                case FontStyle.REVERSE_STYLE:
                    reverse = true;
                    break;

                case FontStyle.BOLDFACE_STYLE:
                case FontStyle.EMPHASIS_STYLE:
                    emphasis = true;
                    break;
            }

                SetConsoleColors();
        }

        private void SelectWindow(int num)
        {
            SaveCursorPos();

            switch (num)
            {
                case 0:
                case 1:
                    _curentWindow = num;
                    break;
                default:
                    return;
            }

            RestoreCursorPos();
            SetConsoleColors();
        }

        private void RestoreCursorPos()
        {
            _consoles[_curentWindow].RestoreCursorPosition();
        }

        private void SaveCursorPos()
        {
            _consoles[_curentWindow].SaveCursorPosition();
        }

        private void MoveCursor(int x, int y)
        {
            // only allowed when upper window is selected
            if (_curentWindow == WindowType.Status)
            {
                if (x < 1)
                {
                    x = 1;
                }
                else if (x > _consoles[_curentWindow].Columns + _consoles[_curentWindow].Left)
                {
                    x = _consoles[_curentWindow].Columns + _consoles[_curentWindow].Left;
                }

                if (y < 1)
                {
                    y = 1;
                }
                else if (y > _consoles[_curentWindow].Rows + _consoles[_curentWindow].Top)
                {
                    y = _consoles[_curentWindow].Columns + _consoles[_curentWindow].Left;
                }

                Console.SetCursorPosition(x - 1, y - 1);
            }
        }

        private ConsoleColor ColorToConsole(short num, ConsoleColor current, bool background)
        {
            switch (num)
            {
                case 0: return current;
                case 1: return background ? ConsoleColor.Black : ConsoleColor.Gray;
                case 2: return ConsoleColor.Black;
                case 3: return ConsoleColor.DarkRed;
                case 4: return ConsoleColor.DarkGreen;
                case 5: return ConsoleColor.DarkYellow;
                case 6: return ConsoleColor.DarkBlue;
                case 7: return ConsoleColor.DarkMagenta;
                case 8: return ConsoleColor.DarkCyan;
                case 9: return ConsoleColor.Gray;
                default:
                    return current;
            }
        }

        private ConsoleColor EmphasizeColor(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black: return ConsoleColor.DarkGray;
                case ConsoleColor.DarkRed: return ConsoleColor.Red;
                case ConsoleColor.DarkGreen: return ConsoleColor.Green;
                case ConsoleColor.DarkYellow: return ConsoleColor.Yellow;
                case ConsoleColor.DarkBlue: return ConsoleColor.Blue;
                case ConsoleColor.DarkMagenta: return ConsoleColor.Magenta;
                case ConsoleColor.DarkCyan: return ConsoleColor.Cyan;
                case ConsoleColor.Gray: return ConsoleColor.White;
                default:
                    return color;
            }
        }

        private void PadStatusLine(int spacesToLeave)
        {
            int x = _consoles[_curentWindow].CursorColumn;
            int xWidth = _consoles[_curentWindow].Columns;

            while (x < xWidth - spacesToLeave)
            {
                WriteChar(' ');
                x++;
            }
        }

        private void CheckMore()
        {
            if (_curentWindow != WindowType.Status && _consoles[_curentWindow].CursorRow == 0)
            {
                lineCount++;
                if (lineCount >= _consoles[_curentWindow].Rows - 1)
                {
                    _consoles[_curentWindow].Write("-- more --");
                    Console.ReadKey(true);

                    // erase the prompt
                    // TODO: I don't think our console likes this.
                    _consoles[_curentWindow].Write("\b\b\b\b\b\b\b\b\b\b");
                    _consoles[_curentWindow].Write("          ");
                    _consoles[_curentWindow].Write("\b\b\b\b\b\b\b\b\b\b");

                    lineCount = 0;
                }
            }
        }
    }
}
