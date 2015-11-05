/*
Copyright (c) 2012-2013, dewitcher Team
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice
   this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;

namespace DuNodes.System.Console
{
    
    /// <summary>
    /// The awesome dewitcher Console
    /// </summary>
    public static partial class Console
    {
        private static int indent = 0;
        /// <summary>
        /// ForegroundColor Property
        /// </summary>
        public static ConsoleColor ForegroundColor { get { return global::System.Console.ForegroundColor; } set { global::System.Console.ForegroundColor = value; } }
        /// <summary>
        /// BackgroundColor Property
        /// </summary>
        public static ConsoleColor BackgroundColor { get { return global::System.Console.BackgroundColor; } set { global::System.Console.BackgroundColor = value; } }
        /// <summary>
        /// CursorTop Property
        /// </summary>
        public static int CursorTop { get { return global::System.Console.CursorTop; } set { global::System.Console.CursorTop = value; } }
        /// <summary>
        /// CursorLeft Property
        /// </summary>
        public static int CursorLeft { get { return global::System.Console.CursorLeft; } set { global::System.Console.CursorLeft = value; } }
        /// <summary>
        /// WindowWidth Property
        /// </summary>
        public static int WindowWidth { get { return global::System.Console.WindowWidth; } set { global::System.Console.WindowWidth = value; } }
        /// <summary>
        /// WindowHeight Property
        /// </summary>
        public static int WindowHeight { get { return global::System.Console.WindowHeight; } set { global::System.Console.WindowHeight = value; } }
        /// <summary>
        /// KeyAvailable Property
        /// </summary>
        public static bool KeyAvailable { get { return global::System.Console.KeyAvailable; } }
        /// <summary>
        /// Write Method
        /// </summary>
        /// <param name="chr">The char to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void Write(char chr = char.MinValue, ConsoleColor color = ConsoleColor.White, bool xcenter = false, bool ycenter = false)
        {
            Write(chr.ToString(), color, xcenter, ycenter);
        }
        /// <summary>
        /// Write Method
        /// </summary>
        /// <param name="chr">The char to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void WriteEx(char chr = char.MinValue, ConsoleColor color = ConsoleColor.White, ConsoleColor backColor = ConsoleColor.Black, bool xcenter = false, bool ycenter = false)
        {
            WriteEx(chr.ToString(), color, backColor, xcenter, ycenter);
        }
        /// <summary>
        /// Write Method
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void Write(string text = "", ConsoleColor color = ConsoleColor.White, bool xcenter = false, bool ycenter = false)
        {
            ConsoleColor originalColor = ForegroundColor;
            ForegroundColor = color;
            int X = CursorLeft + indent;
            if (xcenter) CursorLeft = ((WindowWidth / 2) - (text.Length / 2));
            int Y = CursorTop;
            if (ycenter) CursorTop = ((WindowHeight / 2) - 1);
            global::System.Console.Write(text);
            if (xcenter) CursorLeft = X;
            if (ycenter) CursorTop = Y;
            ForegroundColor = originalColor;
        }
        /// <summary>
        /// Write Method
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="backcolor">The background color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void WriteEx(string text = "", ConsoleColor color = ConsoleColor.White, ConsoleColor backcolor = ConsoleColor.Black, bool xcenter = false, bool ycenter = false)
        {
            ConsoleColor originalColor = ForegroundColor;
            ConsoleColor originalColor2 = BackgroundColor;
            ForegroundColor = color;
            BackgroundColor = backcolor;
            int X = CursorLeft + indent;
            if (xcenter) CursorLeft = ((WindowWidth / 2) - (text.Length / 2));
            int Y = CursorTop;
            if (ycenter) CursorTop = ((WindowHeight / 2) - 1);
            global::System.Console.Write(text);
            if (xcenter) CursorLeft = X;
            if (ycenter) CursorTop = Y;
            ForegroundColor = originalColor;
            BackgroundColor = originalColor2;
        }
        /// <summary>
        /// WriteLine Method
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White, bool xcenter = false, bool ycenter = false)
        {
            ConsoleColor originalColor = ForegroundColor;
            ForegroundColor = color;
            int X = CursorLeft + indent;
            if (xcenter) CursorLeft = ((WindowWidth / 2) - (text.Length / 2));
            int Y = CursorTop;
            if (ycenter) CursorTop = ((WindowHeight / 2) - 1);
            global::System.Console.WriteLine(text);
            if (xcenter) CursorLeft = X;
            if (ycenter) CursorTop = Y;
            ForegroundColor = originalColor;
        }
        /// <summary>
        /// WriteLine Method
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color of the text</param>
        /// <param name="backcolor">The background color of the text</param>
        /// <param name="xcenter">Horizontal centered?</param>
        /// <param name="ycenter">Vertical centered?</param>
        public static void WriteLineEx(string text = "", ConsoleColor color = ConsoleColor.White, ConsoleColor backcolor = ConsoleColor.Black, bool xcenter = false, bool ycenter = false)
        {
            ConsoleColor originalColor = ForegroundColor;
            ConsoleColor originalColor2 = BackgroundColor;
            ForegroundColor = color;
            BackgroundColor = backcolor;
            int X = CursorLeft + indent;
            if (xcenter) CursorLeft = ((WindowWidth / 2) - (text.Length / 2));
            int Y = CursorTop;
            if (ycenter) CursorTop = ((WindowHeight / 2) - 1);
            global::System.Console.WriteLine(text);
            if (xcenter) CursorLeft = X;
            if (ycenter) CursorTop = Y;
            ForegroundColor = originalColor;
            BackgroundColor = originalColor2;
        }
        /// <summary>
        /// Fills the Console Background with a color
        /// </summary>
        /// <param name="color"></param>
        public static void Fill(ConsoleColor color)
        {
            Clear();
            ConsoleColor backup = BackgroundColor;
            BackgroundColor = color;
            for (int i = 0; i < (80 * 25); i++)
            {
                Write(" ");
            }
            BackgroundColor = backup;
            CursorTop = 0;
        }
        /// <summary>
        /// Clear Method
        /// </summary>
        public static void Clear() { global::System.Console.Clear(); }
        /// <summary>
        /// Wipes the first two lines and writes a text (e.g. "YourOSName")
        /// at the horizontal center of the first line
        /// </summary>
        /// <param name="text">The text to write</param>
        /// <param name="color">The color of the text</param>
        public static void PrintAlias(string text, ConsoleColor color)
        {
            int curTop = CursorTop;
            int curLeft = CursorLeft;
            for (int i = 0; i < 2; i++)
            {
                CursorTop = i;
                for (int ix = 0; ix < WindowWidth; ix++) Write(" ");
            }
            CursorTop = 0;
            WriteLine(text, color, true);
            if (curTop >= 2) CursorTop = curTop;
            else CursorTop = 2;
            CursorLeft = curLeft;
        }
        /// <summary>
        /// Inserts a line break
        /// </summary>
        public static void NewLine() { WriteLine(); }
        /// <summary>
        /// System.Console.Read()-Implementation
        /// </summary>
        /// <returns></returns>
        public static int Read() { return global::System.Console.Read(); }
        /// <summary>
        /// System.Console.ReadKey()-Implementation
        /// </summary>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadKey() { return global::System.Console.ReadKey(); }
        /// <summary>
        /// System.Console.ReadLine()-Implementation
        /// </summary>
        /// <returns></returns>
        public static string ReadLine() { return global::System.Console.ReadLine(); }
        /// <summary>
        /// Sets a custom indent
        /// </summary>
        /// <param name="_indent"></param>
        public static void SetIndent(int _indent)
        {
            indent = _indent;
        }
    }
}
