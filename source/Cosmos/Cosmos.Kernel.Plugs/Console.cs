using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Kernel.Plugs
{
    [Plug(Target = typeof(System.Console))]
    class Console
    {
        private static ConsoleColor _background = ConsoleColor.Black;
        private static ConsoleColor _foreground = ConsoleColor.White;

        public static ConsoleColor get_ForegroundColor()
        {
            return _foreground;
        }

        public static void set_ForegroundColor(ConsoleColor value)
        {
            _foreground = value;
            TextScreen.SetColors(_foreground, _background);
        }

        public static ConsoleColor get_BackgroundColor()
        {
            return _background;
        }

        public static void set_BackgroundColor(ConsoleColor value)
        {
            _background = value;
            TextScreen.SetColors(_foreground, _background);
        }

        public static void Beep(int aFrequency, int aDuration)
        {
            if (aFrequency < 37 || aFrequency > 32767)
                throw new ArgumentOutOfRangeException("Frequency must be between 37 and 32767Hz");

            if (aDuration <= 0)
                throw new ArgumentOutOfRangeException("Duration must be more than 0");

            PIT.EnableSound();
            PIT.T2Frequency = (uint)aFrequency;
            PIT.Wait((uint)aDuration);
            PIT.DisableSound();
        }

        public static int get_CursorLeft()
        {
            return TextScreen.CurrentChar;
        }

        public static int get_CursorTop()
        {
            return TextScreen.CurrentRow;
        }

        public static void set_CursorLeft(int x)
        {
            TextScreen.CurrentChar = x;
        }

        public static void set_CursorTop(int y)
        {
            TextScreen.CurrentRow = y;
        }

        public static int get_WindowHeight()
        {
            return TextScreen.Rows;
        }

        public static int get_WindowWidth()
        {
            return TextScreen.Columns;
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear()
        {
            TextScreen.Clear();
        }

        #region Write

        public static void Write(char[] aBuffer)
        {
            Write(aBuffer, 0, aBuffer.Length);
        }

        public static void Write(char aChar)
        {
            TextScreen.WriteChar(aChar);
        }

        public static void Write(Int32 aInt)
        {
            Write(aInt.ToString());
        }

        public static void Write(Int64 aLong)
        {
            Write(aLong.ToString());
        }

        public static void Write(bool aBool)
        {
            Write(aBool.ToString());
        }

        public static void Write(char[] aBuffer, int aIndex, int aCount)
        {
            if (aBuffer == null)
            {
                throw new ArgumentNullException("aBuffer");
            }
            if (aIndex < 0)
            {
                throw new ArgumentOutOfRangeException("aIndex");
            }
            if (aCount < 0)
            {
                throw new ArgumentOutOfRangeException("aCount");
            }
            if ((aBuffer.Length - aIndex) < aCount)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < aCount; i++)
            {
                Write(aBuffer[aIndex + i]);
            }
        }

        public static void Write(string aText)
        {
            for (int i = 0; i < aText.Length; i++)
            {
              if (aText[i] == '\n') {
                TextScreen.NewLine();
                continue;
              }
              if (aText[i] == '\r') {
                continue;
              }
              if (aText[i] == '\t') {
                Write("    ");
                continue;
              }
                TextScreen.WriteChar(aText[i]);
//                break;
            }
        }

        #endregion

        #region WriteLine

        public static void WriteLine()
        {
            TextScreen.NewLine();
        }

        public static void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        public static void WriteLine(bool aBool)
        {
            Write(aBool);
            WriteLine();
        }

        public static void WriteLine(char aChar)
        {
            Write(aChar);
            WriteLine();
        }

        public static void WriteLine(string aLine)
        {
            Write(aLine);
            WriteLine();
        }

        public static void WriteLine(uint aValue)
        {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(int aValue)
        {
            Write(aValue);
            WriteLine();
        }

        public static void WriteLine(char[] aBuffer, int aIndex, int aCount)
        {
            Write(aBuffer, aIndex, aCount);
            WriteLine();
        }

        #endregion

        public static int Read() {
            return Keyboard.ReadChar();
        }

        public static string ReadLine()
        {
            List<char> chars = new List<char>(32);
            char current;
            int currentCount = 0;

            while ((current = Keyboard.ReadChar()) != '\n')
            {
                //Check for "special" keys
                if (current == '\u0968') // Backspace   
                {
                    if (currentCount > 0)
                    {
                        int curCharTemp = TextScreen.CurrentChar;
                        chars.RemoveAt(currentCount - 1);
                        TextScreen.CurrentChar--;

                        //Move characters to the left
                        for (int x = currentCount - 1; x < chars.Count; x++)
                        {
                            Write(chars[x]);
                        }

                        Write(' ');

                        TextScreen.CurrentChar = curCharTemp - 1;

                        currentCount--;
                    }
                    continue;
                }
                else if (current == '\u2190') // Arrow Left
                {
                    if (currentCount > 0)
                    {
                        TextScreen.CurrentChar--;
                        currentCount--;
                    }
                    continue;
                }
                else if (current == '\u2192') // Arrow Right
                {
                    if (currentCount < chars.Count)
                    {
                        TextScreen.CurrentChar++;
                        currentCount++;
                    }
                    continue;
                }

                //Write the character to the screen
                if (currentCount == chars.Count)
                {
                    chars.Add(current); 
                    Write(current);
                    currentCount++;
                }
                else
                {
                    //Insert the new character in the correct location
                    //For some reason, List.Insert() doesn't work properly
                    //so the character has to be inserted manually
                    List<char> temp = new List<char>();

                    for (int x = 0; x < chars.Count; x++)
                    {
                        if (x == currentCount)
                        {
                            temp.Add(current);
                        }

                        temp.Add(chars[x]);
                    }

                    chars = temp;

                    //Shift the characters to the right
                    for (int x = currentCount; x < chars.Count; x++)
                    {
                        Write(chars[x]);
                    }

                    TextScreen.CurrentChar -= (chars.Count - currentCount) - 1;
                    currentCount++;
                }
            }

            WriteLine();

            char[] final = chars.ToArray();

            return new string(final);
        }
    }
}

