using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// Represents the standard console output stream.
    /// </summary>
    public class Console
    {
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';

        /// <summary>
        /// The underlying X cursor location field.
        /// </summary>
        protected int mX = 0;

        /// <summary>
        /// The text cursor location in the X (horizontal) axis.
        /// </summary>
        public int X
        {
            get => mX;
            set
            {
                mX = value;
                UpdateCursor();
            }
        }

        /// <summary>
        /// The underlying Y cursor location field.
        /// </summary>
        protected int mY = 0;

        /// <summary>
        /// Get and set cursor location on Y axis.
        /// </summary>
        public int Y
        {
            get => mY;
            set
            {
                mY = value;
                UpdateCursor();
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
            mText.Clear();
            mX = 0;
            mY = 0;
            UpdateCursor();
        }

        //TODO: This is slow, batch it and only do it at end of updates
        /// <summary>
        /// Update cursor position.
        /// </summary>
        protected void UpdateCursor()
        {
            mText.SetCursorPos(mX, mY);
        }

        /// <summary>
        /// Scrolls the console up and moves the cursor to the start of the line.
        /// </summary>
        private void DoLineFeed()
        {
            mY++;
            mX = 0;
            if (mY == mText.Rows)
            {
                mText.ScrollUp();
                mY--;
            }
            UpdateCursor();
        }

        /// <summary>
        /// Moves the cursor to the start of the line.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn()
        {
            mX = 0;
            UpdateCursor();
        }

        /// <summary>
        /// Print a tab character to the console.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            // Why use space 4 times instead of tab??
            Write(Tab);
        }

        /// <summary>
        /// Write char to the console.
        /// </summary>
        /// <param name="aChar">A char to write</param>
        public void Write(byte aChar)
        {
            mText[mX, mY] = aChar;
            mX++;
            if (mX == mText.Cols)
            {
                DoLineFeed();
            }
            UpdateCursor();
        }

        //TODO: Optimize this
        /// <summary>
        /// Writes the given sequence of ASCII characters in the form of a byte
        /// array to the console.
        /// </summary>
        /// <param name="aText">The byte array to write to the console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] aText)
        {
            if (aText == null)
            {
                return;
            }

            for (int i = 0; i < aText.Length; i++)
            {
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

        /// <summary>
        /// Get or sets the visibility of the cursor.
        /// </summary>
        public bool CursorVisible
        {
            get => mText.GetCursorVisible();
            set => mText.SetCursorVisible(value);
        }
    }
}
