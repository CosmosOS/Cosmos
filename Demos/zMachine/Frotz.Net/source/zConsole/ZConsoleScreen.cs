using System;
using Frotz.Blorb;
using Frotz.Screen;
using Frotz.Constants;
using System.Collections.Generic;
using zword = System.UInt16;

namespace zConsole
{

    public class ZConsoleScreen : Frotz.Screen.IZScreen
    {
        string loaded_file = null;

        public ZConsoleScreen(string file, byte[] bytes)
        {
            loaded_file = file;
            Frotz.os_.SetScreen(this);
            //Frotz.Generic.main.
            Frotz.os_.preloadedFileData = bytes;
            Frotz.Generic.main.MainFunc(new[] { file });
        }


        public EventHandler<ZKeyPressEventArgs> KeyPressed
        {
            get;
            set;
        }

        public void addInputChar(char c)
        {
            KeyPressed?.Invoke(this, new ZKeyPressEventArgs(c));
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void ClearArea(int top, int left, int bottom, int right)
        {
            for(int y = top; y < bottom; y++)
            {
                for(int x = left; x < right; x++)
                {
                    Console.CursorLeft = x;
                    Console.CursorTop = y;
                    Console.Write(" ");
                }
            }
        }

        private string repeat(string input, int len)
        {
            string output = "";
            for(int i = 0; i < len; i++)
            {
                output += input;
            }
            return input;
        }

        string text = "";

        int width = 0;
        int height = 0;

        public void DisplayChar(char c)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (c.Equals('\n'))
            {
                Console.WriteLine();
            }
            else if (c.Equals('\r'))
            {
                c = '\r'; //I have to do something here. C# is dumb.
            }
            else if(c == CharCodes.ZC_GAP)
            {
                Console.Write(" ");
            }
            else
            {
                Console.Write(c);
            }
        }

        public void DisplayMessage(string Message, string Caption)
        {
            throw new NotImplementedException();
        }

        public void DrawPicture(int picture, byte[] Image, int y, int x)
        {
            throw new NotImplementedException();
        }

        public void FinishWithSample(int number)
        {
            throw new NotImplementedException();
        }

        public void GetColor(out int foreground, out int background)
        {
            foreground = (int)Console.ForegroundColor;
            background = (int)Console.BackgroundColor;
        }

        public ZPoint GetCursorPosition()
        {
            return new ZPoint(Console.CursorLeft, Console.CursorTop);
        }

        public bool GetFontData(int font, ref ushort height, ref ushort width)
        {
            //FIXME: No fonts.
            return false;
        }

        public ZSize GetImageInfo(byte[] Image)
        {
            throw new NotImplementedException();
        }

        public ScreenMetrics GetScreenMetrics()
        {
            //FIXME: Find a way to get size of font
            int font_w = 1;
            int font_h = 1;
            int screen_w = Console.WindowWidth;
            int screen_h = Console.WindowHeight;
            var sm = new ScreenMetrics(new ZSize(font_w, font_h), new ZSize(screen_w, screen_h), screen_h, screen_w, screen_w * screen_h);
            return sm;
        }

        public int GetStringWidth(string s, CharDisplayInfo Font)
        {
            return s.Length;
        }

        public void HandleFatalError(string Message)
        {
            DisplayMessage(Message, "Fatal error");
        }

        public string OpenExistingFile(string defaultName, string Title, string Filter)
        {
            throw new NotImplementedException();
        }

        public string OpenNewOrExistingFile(string defaultName, string Title, string Filter, string defaultExtension)
        {
            throw new NotImplementedException();
        }

        public ushort PeekColor()
        {
            throw new NotImplementedException();
        }

        public void PrepareSample(int number)
        {
            throw new NotImplementedException();
        }

        public event EventHandler RestartGame;

        public void RefreshScreen()
        {
            if (in_input)
            {
                Frotz.os_.entries.Clear();
                string input = Console.ReadLine();
                foreach (char c in input)
                {
                    addInputChar(c);
                }
                addInputChar('\r');
                addInputChar('\n'); //I modified Frotz to accept Carriage Return, and Linefeed (\n) as CharCodes.ZC_RETURN.
                in_input = false;
            }
        }

        public void RemoveChars(int count)
        {
            Console.Write(repeat(" ", count));
        }

        public void ScrollArea(int top, int bottom, int left, int right, int units)
        {
            throw new NotImplementedException();
        }

        int offset_y = 0;

        public void ScrollLines(int top, int height, int lines)
        {
            //top, and height, are probably used to not scroll that statusbar.
            //We're not USING said statusbar, so disregard them.
            offset_y += lines;
        }

        public string SelectGameFile(out byte[] filedata)
        {
            throw new NotImplementedException();
        }

        public void SetActiveWindow(int win)
        {
            //TODO: No. Only one window.
        }

        public void SetColor(int new_foreground, int new_background)
        {
            Console.ForegroundColor = (ConsoleColor)new_foreground;
            Console.BackgroundColor = (ConsoleColor)new_background;
        }

        public void SetCursorPosition(int x, int y)
        {
            if (x < 0)
                x = 0;
            if (x >= Console.WindowWidth)
                x = Console.WindowWidth - 1;

            Console.CursorLeft = x;
            if (!in_input)
                Console.CursorTop = y + offset_y;
            else
                Console.CursorTop = y;
        }

        public void SetFont(int font)
        {
            throw new NotImplementedException();
        }

        public void SetInputColor()
        {
            //TODO: What does this do?
        }

        bool in_input = false;

        int ix = 0;
        int iy = 0;

        public ScreenMetrics Metrics
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetInputMode(bool InputMode, bool CursorVisibility)
        {
            Console.CursorVisible = CursorVisibility;
            in_input = InputMode;
            if (in_input)
            {
                ix = Console.CursorLeft;
                iy = Console.CursorTop;
                RefreshScreen();
            }
        }

        public void SetTextStyle(int new_style)
        {
            //Do nothing.
        }

        public void SetWindowSize(int win, int top, int left, int height, int width)
        {
        }

        public bool ShouldWrap()
        {
            //Don't think Console will let you decide if it should wrap.
            return true;
        }

        public void StartSample(int number, int volume, int repeats, ushort eos)
        {
            throw new NotImplementedException();
        }

        public void StopSample(int number)
        {
            throw new NotImplementedException();
        }

        public void StoryStarted(string StoryName, Blorb BlorbFile)
        {
            Console.WriteLine("Starting " + StoryName);
        }

        public void SetCharsAndLines()
        {
            throw new NotImplementedException();
        }

        public void setFontInfo()
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
