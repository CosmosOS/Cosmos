using IL2CPU.API.Attribs;
using Cosmos.HAL.Drivers.Video;
using Cosmos.System.Graphics;
using Cosmos.System;
using System.Text;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof (global::System.Console))]
    public static class ConsoleImpl
    {
        #region Properties

        public static bool TreatControlCAsInput => throw new NotImplementedException("Not implemented: TreatControlCAsInput");

        public static int LargestWindowHeight => throw new NotImplementedException("Not implemented: LargestWindowHeight");

        public static int LargestWindowWidth => throw new NotImplementedException("Not implemented: LargestWindowWidth");

        public static string Title => throw new NotImplementedException("Not implemented: Title");

        public static int BufferHeight => throw new NotImplementedException("Not implemented: BufferHeight");

        public static int BufferWidth => throw new NotImplementedException("Not implemented: BufferWidth");

        public static int WindowLeft => throw new NotImplementedException("Not implemented: WindowLeft");

        public static int WindowTop => throw new NotImplementedException("Not implemented: WindowTop");

        public static Encoding OutputEncoding
        {
            get => consoleOutputEncoding;
            set
            {
                consoleOutputEncoding = value;
            }
        }
        
        public static Encoding InputEncoding
        {
            get => consoleInputEncoding;
            set
            {
                consoleInputEncoding = value;
            }
        }

        public static bool KeyAvailable => KeyboardManager.KeyAvailable;

        public static bool NumberLock => Global.NumLock;

        public static bool CapsLock => Global.CapsLock;

        public static ConsoleColor ForegroundColor
        {
            get => foreGround;
            set
            {
                foreGround = value;

                if (GetConsole() != null)
                {
                    GetConsole().Foreground = value;
                }
            }
        }

        public static ConsoleColor BackgroundColor
        {
            get => backGround;
            set
            {
                backGround = value;

                if (GetConsole() != null)
                {
                    GetConsole().Background = value;
                }
            }
        }

        public static bool CursorVisible
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    return false;
                }
                return GetConsole().CursorVisible;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }
                xConsole.CursorVisible = value;
            }
        }

        public static int CursorSize
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return xConsole.CursorSize;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }
                xConsole.CursorSize = value;
            }
        }

        public static int CursorLeft
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return GetConsole().X;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentException("The value must be at least 0!", nameof(value));
                }

                if (value < WindowWidth)
                {
                    xConsole.X = value;
                }
                else
                {
                    throw new ArgumentException("The value must be lower than the console width!", nameof(value));
                }
            }
        }

        public static int CursorTop
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return GetConsole().Y;
            }
            set
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return;
                }

                if (value < 0)
                {
                    throw new ArgumentException("The value must be at least 0!", nameof(value));
                }

                if (value < WindowHeight)
                {
                    xConsole.Y = value;
                }
                else
                {
                    throw new ArgumentException("The value must be lower than the console height!", nameof(value));
                }
            }
        }

        public static int WindowHeight
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 25;
                }
                return GetConsole().Rows;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowHeight");
        }

        public static int WindowWidth
        {
            get
            {
                var xConsole = GetConsole();
                if (xConsole == null)
                {
                    // for now:
                    return 85;
                }
                return GetConsole().Cols;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowWidth");
        }

        #endregion

        #region Methods

        public static void SetBufferSize(int width, int height)
        {
            throw new NotImplementedException("Not implemented: SetBufferSize");
        }

        public static void SetCursorPosition(int left, int top)
        {
            CursorLeft = left;
            CursorTop = top;
        }

        public static void SetWindowPosition(int left, int top)
        {
            throw new NotImplementedException("Not implemented: SetWindowPosition");
        }

        public static void SetWindowSize(int width, int height)
        {
            if (width == 40 && height == 25)
            {
                fallBackConsole.mText.Cols = 40;
                fallBackConsole.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x25);
            }
            else if (width == 40 && height == 50)
            {
                fallBackConsole.mText.Cols = 40;
                fallBackConsole.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x50);
            }
            else if (width == 80 && height == 25)
            {
                fallBackConsole.mText.Cols = 80;
                fallBackConsole.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
            }
            else if (width == 80 && height == 50)
            {
                fallBackConsole.mText.Cols = 80;
                fallBackConsole.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x50);
            }
            else if (width == 90 && height == 30)
            {
                fallBackConsole.mText.Cols = 90;
                fallBackConsole.mText.Rows = 30;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x30);
            }
            else if (width == 90 && height == 60)
            {
                fallBackConsole.mText.Cols = 90;
                fallBackConsole.mText.Rows = 60;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x60);
            }
            else
            {
                throw new Exception("Invalid text size.");
            }

            fallBackConsole.Cols = fallBackConsole.mText.Cols;
            fallBackConsole.Rows = fallBackConsole.mText.Rows;

            ((HAL.TextScreen)fallBackConsole.mText).UpdateWindowSize();

            Clear();
        }

        public static (int Left, int Top) GetCursorPosition()
        {
            return (CursorLeft, CursorTop);
        }

        //  MoveBufferArea(int, int, int, int, int, int) is pure CIL
        public static void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
        {
            throw new NotImplementedException("Not implemented: MoveBufferArea");
        }

        private static Cosmos.System.Console GetConsole()
        {
            return fallBackConsole;
        }

        // ReadKey() pure CIL
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            var key = KeyboardManager.ReadKey();
            if (intercept == false && key.KeyChar != '\0')
            {
                Write(key.KeyChar);
            }

            //TODO: Plug HasFlag and use the next 3 lines instead of the 3 following lines

            //bool xShift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);
            //bool xAlt = key.Modifiers.HasFlag(ConsoleModifiers.Alt);
            //bool xControl = key.Modifiers.HasFlag(ConsoleModifiers.Control);

            bool xShift = (key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift;
            bool xAlt = (key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt;
            bool xControl = (key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control;

            return new ConsoleKeyInfo(key.KeyChar, key.Key.ToConsoleKey(), xShift, xAlt, xControl);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKey(false);
        }

        public static string ReadLine()
        {
            var xConsole = GetConsole();
            if (xConsole == null)
            {
                // for now:
                return null;
            }
            List<char> chars = new(32);
            KeyEvent current;
            int currentCount = 0;

            while ((current = KeyboardManager.ReadKey()).Key != ConsoleKeyEx.Enter)
            {
                if (current.Key == ConsoleKeyEx.NumEnter)
                {
                    break;
                }
                //Check for "special" keys
                if (current.Key == ConsoleKeyEx.Backspace) // Backspace
                {
                    if (currentCount > 0)
                    {
                        int curCharTemp = GetConsole().X;
                        chars.RemoveAt(currentCount - 1);
                        GetConsole().X = GetConsole().X - 1;

                        //Move characters to the left
                        for (int x = currentCount - 1; x < chars.Count; x++)
                        {
                            Write(chars[x]);
                        }

                        Write(' ');

                        GetConsole().X = curCharTemp - 1;

                        currentCount--;
                    }
                    continue;
                }
                else if (current.Key == ConsoleKeyEx.LeftArrow)
                {
                    if (currentCount > 0)
                    {
                        GetConsole().X = GetConsole().X - 1;
                        currentCount--;
                    }
                    continue;
                }
                else if (current.Key == ConsoleKeyEx.RightArrow)
                {
                    if (currentCount < chars.Count)
                    {
                        GetConsole().X = GetConsole().X + 1;
                        currentCount++;
                    }
                    continue;
                }

                if (current.KeyChar == '\0')
                {
                    continue;
                }

                //Write the character to the screen
                if (currentCount == chars.Count)
                {
                    chars.Add(current.KeyChar);
                    Write(current.KeyChar);
                    currentCount++;
                }
                else
                {
                    //Insert the new character in the correct location
                    //For some reason, List.Insert() doesn't work properly
                    //so the character has to be inserted manually
                    List<char> temp = new();

                    for (int x = 0; x < chars.Count; x++)
                    {
                        if (x == currentCount)
                        {
                            temp.Add(current.KeyChar);
                        }

                        temp.Add(chars[x]);
                    }

                    chars = temp;

                    //Shift the characters to the right
                    for (int x = currentCount; x < chars.Count; x++)
                    {
                        Write(chars[x]);
                    }

                    GetConsole().X -= chars.Count - currentCount - 1;
                    currentCount++;
                }
            }
            WriteLine();

            char[] final = chars.ToArray();
            return new string(final);
        }

        public static void ResetColor()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
        }

        public static int Read()
        {
            if (KeyboardManager.TryReadKey(out KeyEvent result))
            {
                return result.KeyChar;
            }
            else
            {
                return -1;
            }
        }

        public static void Beep(int frequency, int duration)
        {
            PCSpeaker.Beep((uint)frequency, (uint)duration);
        }

        /// <summary>
        /// Beep() is pure CIL
        /// Default implementation beeps for 200 milliseconds at 800 hertz
        /// In Cosmos, these are Cosmos.System.Duration.Default and Cosmos.System.Notes.Default respectively,
        /// and are used when there are no params
        /// https://docs.microsoft.com/en-us/dotnet/api/system.console.beep?view=netcore-2.0
        /// </summary>
        public static void Beep()
        {
            PCSpeaker.Beep();
        }

        //TODO: Console uses TextWriter - intercept and plug it instead
        public static void Clear()
        {
            var xConsole = GetConsole();
            if (xConsole == null)
            {
                // for now:
                return;
            }
            GetConsole().Clear();
        }

        #region WriteLine

        public static void WriteLine() => Write(Environment.NewLine);

        public static void WriteLine(bool aBool) => WriteLine(aBool.ToString());

        public static void WriteLine(char aChar) => WriteLine(aChar.ToString());

        public static void WriteLine(char[] aBuffer) => WriteLine(new string(aBuffer));

        /* Decimal type is not working yet... */
        //public static void WriteLine(decimal aDecimal) => WriteLine(aDecimal.ToString());

        public static void WriteLine(double aDouble) => WriteLine(aDouble.ToString());

        public static void WriteLine(float aFloat) => WriteLine(aFloat.ToString());

        public static void WriteLine(int aInt) => WriteLine(aInt.ToString());

        public static void WriteLine(long aLong) => WriteLine(aLong.ToString());

        /* Correct behaviour printing null should not throw NRE or do nothing but should print an empty line */
        public static void WriteLine(object value) => Write((value ?? string.Empty).ToString() + Environment.NewLine);

        public static void WriteLine(string aText) => Write(aText + Environment.NewLine);

        public static void WriteLine(uint aInt) => WriteLine(aInt.ToString());

        public static void WriteLine(ulong aLong) => WriteLine(aLong.ToString());

        public static void WriteLine(string format, object arg0) => WriteLine(string.Format(format, arg0));

        public static void WriteLine(string format, object arg0, object arg1) => WriteLine(string.Format(format, arg0, arg1));

        public static void WriteLine(string format, object arg0, object arg1, object arg2) => WriteLine(string.Format(format, arg0, arg1, arg2));

        public static void WriteLine(string format, params object[] arg) => WriteLine(string.Format(format, arg));

        public static void WriteLine(char[] aBuffer, int aIndex, int aCount)
        {
            Write(aBuffer, aIndex, aCount);
            WriteLine();
        }

        #endregion

        #region Write

        public static void Write(bool aBool)
        {
            Write(aBool.ToString());
        }

        /*
         * A .Net character can be effectevily more can one byte so calling the low level Console.Write() will be wrong as
         * it accepts only bytes, we need to convert it using the specified OutputEncoding but to do this we have to convert
         * it ToString first
         */
        public static void Write(char aChar) => Write(aChar.ToString());

        public static void Write(char[] aBuffer) => Write(aBuffer, 0, aBuffer.Length);

        /* Decimal type is not working yet... */
        //public static void Write(decimal aDecimal) => Write(aDecimal.ToString());

        public static void Write(double aDouble) => Write(aDouble.ToString());

        public static void Write(float aFloat) => Write(aFloat.ToString());

        public static void Write(int aInt) => Write(aInt.ToString());

        public static void Write(long aLong) => Write(aLong.ToString());

        /* Correct behaviour printing null should not throw NRE or do nothing but should print an empty string */
        public static void Write(object value) => Write((value ?? string.Empty).ToString());

        public static void Write(string aText)
        {
            var xConsole = GetConsole();
            if (xConsole == null)
            {
                // for now:
                return;
            }

            byte[] aTextEncoded = consoleOutputEncoding.GetBytes(aText);
            GetConsole().Write(aTextEncoded);
        }

        public static void Write(uint aInt) => Write(aInt.ToString());

        public static void Write(ulong aLong) => Write(aLong.ToString());

        public static void Write(string format, object arg0) => Write(string.Format(format, arg0));

        public static void Write(string format, object arg0, object arg1) => Write(string.Format(format, arg0, arg1));

        public static void Write(string format, object arg0, object arg1, object arg2) => Write(string.Format(format, arg0, arg1, arg2));

        public static void Write(string format, params object[] arg) => Write(string.Format(format, arg));

        public static void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (buffer.Length - index < count)
            {
                throw new ArgumentException();
            }
            for (int i = 0; i < count; i++)
            {
                Write(buffer[index + i]);
            }
        }

        //You'd expect this to be on System.Console wouldn't you? Well, it ain't so we just rely on Write(object value)
        //public static void Write(byte aByte) {
        //    Write(aByte.ToString());
        //}

        #endregion

        #endregion

        #region Fields

        private static readonly Cosmos.System.Console fallBackConsole = new(null);
        private static Encoding consoleOutputEncoding = Encoding.ASCII;
        private static Encoding consoleInputEncoding = Encoding.ASCII;
        private static ConsoleColor foreGround = ConsoleColor.White;
        private static ConsoleColor backGround = ConsoleColor.Black;

        #endregion
    }
}
