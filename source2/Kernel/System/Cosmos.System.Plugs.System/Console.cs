using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System {
    [Plug(Target = typeof(System.Console))]
    public class Console {
        private static ConsoleColor _background = ConsoleColor.Black;
        private static ConsoleColor _foreground = ConsoleColor.White;

        public static ConsoleColor get_ForegroundColor() {
            return _foreground;
        }

        public static void set_ForegroundColor(ConsoleColor value) {
            _foreground = value;
            Global.TextScreen.SetColors(_foreground, _background);
        }

        public static ConsoleColor get_BackgroundColor() {
            return _background;
        }

        public static void set_BackgroundColor(ConsoleColor value) {
            _background = value;
            Global.TextScreen.SetColors(_foreground, _background);
        }

        public static void Beep(int aFrequency, int aDuration) {
            if (aFrequency < 37 || aFrequency > 32767) {
                throw new ArgumentOutOfRangeException("Frequency must be between 37 and 32767Hz");
            }

            if (aDuration <= 0) {
                throw new ArgumentOutOfRangeException("Duration must be more than 0");
            }
            
            Hardware2.PIT.EnableSound();
            Hardware2.PIT.T2Frequency = (uint)aFrequency;
            Hardware2.PIT.Wait((uint)aDuration);
            Hardware2.PIT.DisableSound();
        }

        public static int get_CursorLeft() {
            return Global.TextScreen.CurrentX;
        }

        public static int get_CursorTop() {
            return Global.TextScreen.CurrentY;
        }

        public static void set_CursorLeft(int x) {
            Global.TextScreen.CurrentX = x;
        }

        public static void set_CursorTop(int y) {
            Global.TextScreen.CurrentY = y;
        }

        public static int get_WindowHeight() {
            return Global.TextScreen.Rows;
        }

        public static int get_WindowWidth() {
            return Global.TextScreen.Columns;
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear() {
            Global.TextScreen.Clear();
        }

        #region Write

        public static void Write(char[] aBuffer) {
            Write(aBuffer, 0, aBuffer.Length);
        }

        public static void Write(char aChar) {
            Global.TextScreen.WriteChar(aChar);
        }

        public static void Write(byte aByte) {
            Write(aByte.ToString());
        }

        public static void Write(UInt32 aInt) {
            Write(aInt.ToString());
        }

        public static void Write(Int32 aInt) {
            Write(aInt.ToString());
        }

        public static void Write(UInt16 aInt) {
            Write(aInt.ToString());
        }

        public static void Write(Int16 aInt) {
            Write(aInt.ToString());
        }

        public static void Write(UInt64 aLong) {
            Write(aLong.ToString());
        }

        public static void Write(Int64 aLong) {
            Write(aLong.ToString());
        }

        public static void Write(bool aBool) {
            Write(aBool.ToString());
        }

        public static void Write(object value) {
            if (value != null) {
                Write(value.ToString());
            }
        }

        public static void Write(char[] aBuffer, int aIndex, int aCount) {
            if (aBuffer == null) {
                throw new ArgumentNullException("aBuffer");
            }
            if (aIndex < 0) {
                throw new ArgumentOutOfRangeException("aIndex");
            }
            if (aCount < 0) {
                throw new ArgumentOutOfRangeException("aCount");
            }
            if ((aBuffer.Length - aIndex) < aCount) {
                throw new ArgumentException();
            }
            for (int i = 0; i < aCount; i++) {
                Write(aBuffer[aIndex + i]);
            }
        }

        public static void Write(string aText) {
            for (int i = 0; i < aText.Length; i++) {
                if (aText[i] == '\n') {
                    Global.TextScreen.NewLine();
                    continue;
                }
                if (aText[i] == '\r') {
                    continue;
                }
                if (aText[i] == '\t') {
                    Write("    ");
                    continue;
                }
                Global.TextScreen.WriteChar(aText[i]);
            }
        }

        #endregion

        #region WriteLine

        public static void WriteLine() {
            Global.TextScreen.NewLine();
        }

        public static void WriteLine(object value) {
            Write(value);
            WriteLine();
        }

        public static void WriteLine(char[] buffer) {
            Write(buffer);
            WriteLine();
        }

        public static void WriteLine(bool aBool) {
            Write(aBool);
            WriteLine();
        }

        public static void WriteLine(char aChar) {
            Write(aChar);
            WriteLine();
        }

        public static void WriteLine(byte aByte) {
            Write(aByte);
            WriteLine();
        }

        public static void WriteLine(string aLine) {
            Write(aLine);
            WriteLine();
        }

        public static void WriteLine(UInt16 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(Int16 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(UInt32 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(Int32 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(UInt64 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(Int64 aValue) {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(char[] aBuffer, int aIndex, int aCount) {
            Write(aBuffer, aIndex, aCount);
            WriteLine();
        }

        #endregion

        public static int Read() {
            return Cosmos.Hardware2.Keyboard.ReadChar();
        }

        public static string ReadLine() {
            List<char> chars = new List<char>(32);
            char current;
            int currentCount = 0;

            while ((current = Cosmos.Hardware2.Keyboard.ReadChar()) != '\n') {
                //Check for "special" keys
                if (current == '\u0968') // Backspace   
                {
                    if (currentCount > 0) {
                        int curCharTemp = Global.TextScreen.CurrentX;
                        chars.RemoveAt(currentCount - 1);
                        Global.TextScreen.CurrentX--;

                        //Move characters to the left
                        for (int x = currentCount - 1; x < chars.Count; x++) {
                            Write(chars[x]);
                        }

                        Write(' ');

                        Global.TextScreen.CurrentX = curCharTemp - 1;

                        currentCount--;
                    }
                    continue;
                } else if (current == '\u2190') // Arrow Left
                {
                    if (currentCount > 0) {
                        Global.TextScreen.CurrentX--;
                        currentCount--;
                    }
                    continue;
                } else if (current == '\u2192') // Arrow Right
                {
                    if (currentCount < chars.Count) {
                        Global.TextScreen.CurrentX++;
                        currentCount++;
                    }
                    continue;
                }

                //Write the character to the screen
                if (currentCount == chars.Count) {
                    chars.Add(current);
                    Write(current);
                    currentCount++;
                } else {
                    //Insert the new character in the correct location
                    //For some reason, List.Insert() doesn't work properly
                    //so the character has to be inserted manually
                    List<char> temp = new List<char>();

                    for (int x = 0; x < chars.Count; x++) {
                        if (x == currentCount) {
                            temp.Add(current);
                        }

                        temp.Add(chars[x]);
                    }

                    chars = temp;

                    //Shift the characters to the right
                    for (int x = currentCount; x < chars.Count; x++) {
                        Write(chars[x]);
                    }

                    Global.TextScreen.CurrentX -= (chars.Count - currentCount) - 1;
                    currentCount++;
                }
            }

            WriteLine();

            char[] final = chars.ToArray();

            return new string(final);
        }
    }
}
