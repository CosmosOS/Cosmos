using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.HAL;
using Cosmos.System.IO;
using static Cosmos.System.Console;

#nullable enable
namespace Cosmos.System
{
    /// <summary>
    /// Represents the standard console output stream.
    /// </summary>
    public partial class Console
    {
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';
        private const int WriteBufferSize = 256;
        private const int NumberOfSpacesForTab = 4;

        private SyncTextReader? _stdInReader;
        private Stream? stdOut, stdIn, stdError;

        /// <summary>
        /// The underlying X cursor location field.
        /// </summary>
        protected int mX = 0;
        /// <summary>
        /// The underlying Cached X cursor location field.
        /// </summary>
        protected int cX = 0;

        /// <summary>
        /// The text cursor location in the X (horizontal) axis.
        /// </summary>
        public int X
        {
            get => mX;
            set
            {
                if (value < 0)
                {
                    if (mY > 0)
                    {
                        mY--;
                    }

                    mX = Cols - 1;
                }
                else if (value >= mText.Cols)
                {
                    cY = mY;
                    DoLineFeed();
                    mY = cY;
                    mX = cX;
                }
                else
                {

                    mX = value;
                }
                UpdateCursor();
            }
        }

        /// <summary>
        /// The text cached cursor location in the X (horizontal) axis.
        /// </summary>
        public int CachedX
        {
            get => cX;
            set
            {
                if (value < 0)
                {
                    if (cY > 0)
                    {
                        cY--;
                    }

                    cX = mText.Cols - 1;
                }
                else if(value >= mText.Cols)
                {
                    DoLineFeed();
                }
                else
                {
                
                    cX = value;
                }
            }
        }

        /// <summary>
        /// The underlying Y cursor location field.
        /// </summary>
        protected int mY = 0;
        /// <summary>
        /// The underlying Cached Y cursor location field
        /// </summary>
        protected int cY = 0;

        /// <summary>
        /// Get and set cursor location on Y axis.
        /// </summary>
        public int Y
        {
            get => mY;
            set
            {
                if (value < 0)
                {
                    mY = 0;
                }
                else if (value >= mText.Rows)
                {
                    cY = mText.Rows - 1;
                    DoLineFeed();
                }
                else
                {
                    cY = value;
                }
                UpdateCursor();
            }
        }

        public int CachedY
        {
            get => cY;
            set
            {
                if (value < 0)
                {
                    cY = 0;
                }
                else if (value >= mText.Rows)
                {
                    cY = mText.Rows - 1;
                    DoLineFeed();
                    UpdateCursorFromCache();
                }
                else
                {
                    cY = value;
                }
            }
        }

        /// <summary>
        /// Get window width.
        /// </summary>
        public int Cols
        {
            get => mText.Cols;
            set { }
        }

        /// <summary>
        /// Get window height.
        /// </summary>
        public int Rows
        {
            set { }
            get => mText.Rows;
        }

        /// <summary>
        /// Text screen.
        /// </summary>
        public HAL.TextScreenBase mText;

        /// <summary>
        /// Constructs a new instance of the <see cref="Console"/> class.
        /// </summary>
        /// <param name="textScreen">The device to direct text output to.</param>
        public Console(TextScreenBase textScreen)
        {
            if (textScreen == null)
            {
                mText = new TextScreen();
            }
            else
            {
                mText = textScreen;
            }
        }

        /// <summary>
        /// Clears the console, and changes the cursor location to (0, 0).
        /// </summary>
        public void Clear()
        {
            if (!IsStdOutRedirected())
            {
                mText.Clear();
                cX = cY = mX = mY = 0;
                UpdateCursor();
            }
        }

        //TODO: This is slow, batch it and only do it at end of updates
        /// <summary>
        /// Update cursor position.
        /// </summary>
        protected void UpdateCursor()
        {
            cX = mX;
            cY = mY;
            mText.SetCursorPos(mX, mY);
        }

        public void UpdateCursorFromCache()
        {
            mX = cX;
            mY = cY;
            mText.SetCursorPos(mX, mY);
        }

        /// <summary>
        /// Scrolls the console up and moves the cursor to the start of the line.
        /// </summary>
        private void DoLineFeed()
        {
            cY++;
            cX = 0;
            if (cY == mText.Rows)
            {
                mText.ScrollUp();
                cY--;
            }
        }

        /// <summary>
        /// Moves the cursor to the start of the line.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn()
        {
            cX = 0;
        }

        /// <summary>
        /// Print a tab character to the console.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            // Calculate the remainder when CachedX is divided by the NumberOfSpacesForTab
            int remainder = cX % NumberOfSpacesForTab;

            // if remainder is 0 then we will do a complete new tab, otherwise we will do what needs to be done to complete a tab
            remainder = (remainder == 0) ? NumberOfSpacesForTab : (NumberOfSpacesForTab - remainder);

            for (int i = 0; i < remainder; i++)
            {
                Write(Space);
            }
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(byte aChar)
        {
            Write(aChar, cX, cY);
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write.</param>
        /// <param name="left">X axis position.</param>
        /// <param name="top">Y axis position.</param>
        public void Write(byte aChar, int left, int top)
        {
            mText[left, top] = aChar;
            cX++;
            if (cX == mText.Cols)
            {
                DoLineFeed();
            }
        }

        //TODO: Optimize this
        /// <summary>
        /// Writes the given sequence of ASCII characters in the form of a byte
        /// array to the console.
        /// </summary>
        /// <param name="aText">The byte array to write to the console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> aText)
        {
            if (aText == null)
            {
                return;
            }

            for (int i = 0; i < aText.Length; i++)
            {
                if (aText[i] == '\0')
                {
                    // Skipping Null chars given by StreamWriter
                    continue;
                }

                switch (aText[i])
                {
                    case LineFeed:
                        DoLineFeed();
                        break;

                    case CarriageReturn:
                        DoCarriageReturn();
                        break;

                    case Tab:
                        DoTab();
                        break;

                    /* Normal characters, simply write them */
                    default:
                        Write(aText[i]);
                        break;
                }
            }
        }
        /// <summary>
        /// The foreground color of the displayed text.
        /// </summary>
        public ConsoleColor Foreground
        {
            get => (ConsoleColor)(mText.GetColor() ^ (byte)((byte)Background << 4));
            set => mText.SetColors(value, Background);
        }

        /// <summary>
        /// The background color of the displayed text.
        /// </summary>
        public ConsoleColor Background
        {
            get => (ConsoleColor)(mText.GetColor() >> 4);
            set => mText.SetColors(Foreground, value);
        }

        /// <summary>
        /// The size of the cursor, in the range of 1 to 100.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to set value out of range.</exception>
        public int CursorSize
        {
            get => mText.GetCursorSize();
            set
            {
                // Value should be a percentage from [1, 100].
                if (value is < 1 or > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The given CursorSize value " + value + " is out of range (1 - 100).");
                }

                mText.SetCursorSize(value);
            }
        }

        public Stream OpenStandardInput()
        {
            return stdIn ??= new CosmosConsoleStream(FileAccess.Read);
        }

        public Stream OpenStandardOutput()
        {
            return stdOut ??= new CosmosConsoleStream(FileAccess.Write);
        }

        public Stream OpenStandardError()
        {
            return stdError ??= new CosmosConsoleStream(FileAccess.Write);
        }

        public static bool IsStdInRedirected()
        {
            return global::System.Console.In switch
            {
                null => false /* only used on create, if GetOrCreateReader is changed then this may be not needed */,
                SyncTextReader sync => !sync.IsStdIn,
                _ => true
            };
        }

        public static bool IsStdOutRedirected()
        {
            return !(global::System.Console.Out is StreamWriter streamWriter
                    && streamWriter.BaseStream is Console.CosmosConsoleStream);
        }

        public static bool IsStdErrorRedirected()
        {
            return !(global::System.Console.Error is StreamWriter streamWriter
                    && streamWriter.BaseStream is Console.CosmosConsoleStream);
        }

        public TextReader GetOrCreateReader()
        {
            return StdInReader;
        }
        public TextWriter CreateOutputWriter(Stream outputStream) => outputStream == Stream.Null ?
               TextWriter.Null :
               (new StreamWriter(
                   stream: outputStream,
                   encoding: global::System.Console.OutputEncoding.RemovePreamble(),
                   bufferSize: WriteBufferSize,
                   leaveOpen: true)
                {
                    AutoFlush = true
                });
        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            if (global::System.Console.IsInputRedirected)
            {
                // The biggest lie i have seen in my life...
                throw new InvalidOperationException("Can not read console keys as input is redirected.");
            }

            ConsoleKeyInfo keyInfo = StdInReader.ReadKey(out bool previouslyProcessed);

            if (!intercept && !previouslyProcessed && keyInfo.KeyChar != '\0')
            {
                global::System.Console.Write(keyInfo.KeyChar);
            }
            return keyInfo;
        }

        public void ResetInternalStdIn()
        {
            _stdInReader = null;
        }

        internal SyncTextReader StdInReader => _stdInReader ??= SyncTextReader
                    .GetSynchronizedTextReader(new StdInReader(global::System.Console.InputEncoding));


        /// <summary>
        /// Get or sets the visibility of the cursor.
        /// </summary>
        public bool CursorVisible
        {
            get => mText.GetCursorVisible();
            set => mText.SetCursorVisible(value);
        }
    }
    internal static class EncodingExtensions
    {
        public static Encoding RemovePreamble(this Encoding encoding)
        {
            if (encoding.Preamble.Length == 0)
            {
                return encoding;
            }
            return new ConsoleEncoding(encoding);
        }
    }
}
