using IL2CPU.API.Attribs;
using Cosmos.HAL.Drivers.Video;
using Cosmos.System.Graphics;
using Cosmos.System;
using System.Text;
using Cosmos.HAL.BlockDevice;
using Cosmos.System.IO;

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
                ArgumentNullException.ThrowIfNull(value);

                if (s_out != null && !IsOutputRedirected)
                {
                    s_out.Flush();
                    s_out = null;
                }

                if (s_error != null && !IsErrorRedirected)
                {
                    s_error.Flush();
                    s_error = null;
                }

                consoleOutputEncoding = (Encoding)value.Clone();
            }
        }

        public static Encoding InputEncoding
        {
            get => consoleInputEncoding;
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                consoleInputEncoding = (Encoding)value.Clone();
                s_in = null;
                Global.Console.ResetInternalStdIn(); // StdIn Should also reflect the encoding changes the next time it is called.
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

                if (Global.Console != null)
                {
                    Global.Console.Foreground = value;
                }
            }
        }

        public static ConsoleColor BackgroundColor
        {
            get => backGround;
            set
            {
                backGround = value;

                if (Global.Console != null)
                {
                    Global.Console.Background = value;
                }
            }
        }

        public static bool CursorVisible
        {
            get
            {
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    return false;
                }
                return Global.Console.CursorVisible;
            }
            set
            {
                var xConsole = Global.Console;
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
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return xConsole.CursorSize;
            }
            set
            {
                var xConsole = Global.Console;
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
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return Global.Console.X;
            }
            set
            {
                var xConsole = Global.Console;
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
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    // for now:
                    return 0;
                }
                return Global.Console.Y;
            }
            set
            {
                var xConsole = Global.Console;
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
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    // for now:
                    return 25;
                }
                return Global.Console.Rows;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowHeight");
        }

        public static int WindowWidth
        {
            get
            {
                var xConsole = Global.Console;
                if (xConsole == null)
                {
                    // for now:
                    return 85;
                }
                return Global.Console.Cols;
            }
            set => throw new NotImplementedException("Not implemented: set_WindowWidth");
        }

        public static TextReader In => s_in ??= Global.Console.GetOrCreateReader();

        public static TextWriter Out => s_out ??= CreateOutputWriter(OpenStandardOutput());

        public static TextWriter Error => s_error ??= CreateOutputWriter(OpenStandardError());

        public static bool IsOutputRedirected => isOutputRedirected ??= Cosmos.System.Console.IsStdOutRedirected();

        public static bool IsInputRedirected => isInputRedirected ??= Cosmos.System.Console.IsStdInRedirected();

        public static bool IsErrorRedirected => isErrorRedirected ??= Cosmos.System.Console.IsStdErrorRedirected();
        #endregion

        #region Methods
        public static Stream OpenStandardInput(int bufferSize)
        {
            // We do not really use bufferSize, but this method should still be plugged.
            if(bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer Size must be non negative.");
            }
            return Global.Console.OpenStandardInput();
        }
        public static Stream OpenStandardInput() => Global.Console.OpenStandardInput();
        public static Stream OpenStandardOutput(int bufferSize)
        {
            // We do not really use bufferSize, but this method should still be plugged.
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer Size must be non negative.");
            }
            return Global.Console.OpenStandardOutput();
        }
        public static Stream OpenStandardOutput() => Global.Console.OpenStandardOutput();
        public static Stream OpenStandardError(int bufferSize)
        {
            // We do not really use bufferSize, but this method should still be plugged.
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer Size must be non negative.");
            }

            return Global.Console.OpenStandardError();
        }

        public static Stream OpenStandardError() => Global.Console.OpenStandardError();

        public static void SetIn(TextReader newIn)
        {
            ArgumentNullException.ThrowIfNull(newIn, nameof(newIn));
            newIn = SyncTextReader.GetSynchronizedTextReader(newIn);
            s_in = newIn;
            isInputRedirected = Cosmos.System.Console.IsStdInRedirected();
        }

        public static void SetOut(TextWriter newOut)
        {
            ArgumentNullException.ThrowIfNull(newOut, nameof(newOut));
            s_out = newOut;
            isOutputRedirected = Cosmos.System.Console.IsStdOutRedirected();
        }

        public static void SetError(TextWriter newError)
        {
            ArgumentNullException.ThrowIfNull(newError, nameof(newError));
            s_error = newError;
            isErrorRedirected = Cosmos.System.Console.IsStdErrorRedirected();
        }
        public static TextWriter CreateOutputWriter(Stream stream) => Global.Console.CreateOutputWriter(stream);

        public static void SetBufferSize(int width, int height)
        {
            throw new NotImplementedException("Not implemented: SetBufferSize");
        }

        public static void SetCursorPosition(int left, int top)
        {
            Global.Console.CachedX = left;
            Global.Console.CachedY = top;
            Global.Console.UpdateCursorFromCache();
        }

        public static void SetWindowPosition(int left, int top)
        {
            throw new NotImplementedException("Not implemented: SetWindowPosition");
        }

        public static void SetWindowSize(int width, int height)
        {
            if (width == 40 && height == 25)
            {
                Global.Console.mText.Cols = 40;
                Global.Console.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x25);
            }
            else if (width == 40 && height == 50)
            {
                Global.Console.mText.Cols = 40;
                Global.Console.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size40x50);
            }
            else if (width == 80 && height == 25)
            {
                Global.Console.mText.Cols = 80;
                Global.Console.mText.Rows = 25;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x25);
            }
            else if (width == 80 && height == 50)
            {
                Global.Console.mText.Cols = 80;
                Global.Console.mText.Rows = 50;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size80x50);
            }
            else if (width == 90 && height == 30)
            {
                Global.Console.mText.Cols = 90;
                Global.Console.mText.Rows = 30;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x30);
            }
            else if (width == 90 && height == 60)
            {
                Global.Console.mText.Cols = 90;
                Global.Console.mText.Rows = 60;
                VGAScreen.SetTextMode(VGADriver.TextSize.Size90x60);
            }
            else
            {
                throw new Exception("Invalid text size.");
            }

            Global.Console.Cols = Global.Console.mText.Cols;
            Global.Console.Rows = Global.Console.mText.Rows;

            ((HAL.TextScreen)Global.Console.mText).UpdateWindowSize();

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

        // ReadKey() pure CIL
        public static ConsoleKeyInfo ReadKey(bool intercept)
        {
            return Global.Console.ReadKey(intercept);
        }

        public static ConsoleKeyInfo ReadKey()
        {
            return ReadKey(false);
        }

        public static void ResetColor()
        {
            BackgroundColor = ConsoleColor.Black;
            ForegroundColor = ConsoleColor.White;
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

        public static void Clear()
        {
            Global.Console.Clear();
        }

        #endregion

        #region Fields

        private static TextReader? s_in;
        private static TextWriter? s_out, s_error;
        private static bool? isInputRedirected;
        private static bool? isOutputRedirected;
        private static bool? isErrorRedirected;
        private static Encoding consoleOutputEncoding = Encoding.ASCII;
        private static Encoding consoleInputEncoding = Encoding.ASCII;
        private static ConsoleColor foreGround = ConsoleColor.White;
        private static ConsoleColor backGround = ConsoleColor.Black;

        #endregion
    }
}
