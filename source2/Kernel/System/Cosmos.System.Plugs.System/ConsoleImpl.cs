using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System;

namespace Cosmos.System.Plugs.System {
    [Plug(Target = typeof(global::System.Console))]
    public class ConsoleImpl {

        //public static void set_ForegroundColor(ConsoleColor value) {
        //    _foreground = value;
        //    HW.Global.TextScreen.SetColors(_foreground, _background);
        //}

        //public static ConsoleColor get_BackgroundColor() {
        //    return _background;
        //}

        //public static void set_BackgroundColor(ConsoleColor value) {
        //    _background = value;
        //    HW.Global.TextScreen.SetColors(_foreground, _background);
        //}

        public static void Beep(int aFrequency, int aDuration) {
            //if (aFrequency < 37 || aFrequency > 32767) {
            //    throw new ArgumentOutOfRangeException("Frequency must be between 37 and 32767Hz");
            //}

            //if (aDuration <= 0) {
            //    throw new ArgumentOutOfRangeException("Duration must be more than 0");
            //}

            //var xPIT = Hardware.Global.PIT;
            //xPIT.EnableSound();
            //xPIT.T2Frequency = (uint)aFrequency;
            //xPIT.Wait((uint)aDuration);
            //xPIT.DisableSound();
        }

        public static int get_CursorLeft() {
            return Global.Console.X;
        }

        public static int get_CursorTop() {
            return Global.Console.Y;
        }

        public static void set_CursorLeft(int x) {
            Global.Console.X = x;
        }

        public static void set_CursorTop(int y) {
            Global.Console.Y = y;
        }

        public static int get_WindowHeight() {
            return Global.Console.Rows;
        }

        public static int get_WindowWidth() {
            return Global.Console.Cols;
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear() {
            Global.Console.Clear();
        }

        #region Write

        public static void Write(char[] aBuffer) {
            Write(aBuffer, 0, aBuffer.Length);
        }

        public static void Write(char aChar) {
            Global.Console.WriteChar(aChar);
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
            Global.Console.Write(aText);
        }

        #endregion

        #region WriteLine

        public static void WriteLine() {
            Global.Console.NewLine();
        }

        public static void WriteLine(object value) {
            Write(value);
            Global.Console.NewLine();
        }

        public static void WriteLine(char[] buffer) {
            Write(buffer);
            Global.Console.NewLine();
        }

        public static void WriteLine(bool aBool) {
            Write(aBool);
            Global.Console.NewLine();
        }

        public static void WriteLine(char aChar) {
            Write(aChar);
            Global.Console.NewLine();
        }

        public static void WriteLine(byte aByte) {
            Write(aByte);
            Global.Console.NewLine();
        }

        public static void WriteLine(string aLine) {
            Write(aLine);
            Global.Console.NewLine();
        }

        public static void WriteLine(UInt16 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(Int16 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(UInt32 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(Int32 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(UInt64 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(Int64 aValue) {
            Write(aValue);
            Global.Console.NewLine();
        }

        public static void WriteLine(char[] aBuffer, int aIndex, int aCount) {
            Write(aBuffer, aIndex, aCount);
            Global.Console.NewLine();
        }

        #endregion

        public static int Read() {
            //TODO - after this and kb, remove asm ref to Hardware2
            //return Cosmos.Hardware2.Keyboard.ReadChar();
            return 0;
        }

        public static string ReadLine() {
            List<char> chars = new List<char>(32);
            char current;
            int currentCount = 0;

            while ((current = Hardware.Global.Keyboard.ReadChar()) != '\n') {
                //Check for "special" keys
                if (current == '\u0968') // Backspace   
                {
                    if (currentCount > 0) {
                        int curCharTemp = Global.Console.X;
                        chars.RemoveAt(currentCount - 1);
                        Global.Console.X = Global.Console.X - 1;

                        //Move characters to the left
                        for (int x = currentCount - 1; x < chars.Count; x++) {
                            Write(chars[x]);
                        }

                        Write(' ');

                        Global.Console.X = curCharTemp - 1;

                        currentCount--;
                    }
                    continue;
                } else if (current == '\u2190') // Arrow Left
                {
                    if (currentCount > 0) {
                        Global.Console.X = Global.Console.X - 1;
                        currentCount--;
                    }
                    continue;
                } else if (current == '\u2192') // Arrow Right
                {
                    if (currentCount < chars.Count) {
                        Global.Console.X = Global.Console.X + 1;
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

                    Global.Console.X -= (chars.Count - currentCount) - 1;
                    currentCount++;
                }
            }
            WriteLine();

            char[] final = chars.ToArray();
            return new string(final);
        }
    }
}
