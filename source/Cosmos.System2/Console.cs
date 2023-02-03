using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System
{
    /// <summary>
    /// Standard output stream.
    /// </summary>
    public class Console
    {
        /// <summary>
        /// Line feed.
        /// </summary>
        private const byte LineFeed = (byte)'\n';
        /// <summary>
        /// Carriage return.
        /// </summary>
        private const byte CarriageReturn = (byte)'\r';
        /// <summary>
        /// Tab.
        /// </summary>
        private const byte Tab = (byte)'\t';
        /// <summary>
        /// Space.
        /// </summary>
        private const byte Space = (byte)' ';

        /// <summary>
        /// Cursor location on X axis.
        /// </summary>
        protected int mX = 0;

        /// <summary>
        /// Get and set cursor location on X axis.
        /// </summary>
        public int X
        {
            get { return mX; }
            set
            {
                mX = value;
                UpdateCursor();
            }
        }

        /// <summary>
        /// Cursor location on Y axis.
        /// </summary>
        protected int mY = 0;

        /// <summary>
        /// Get and set cursor location on Y axis.
        /// </summary>
        public int Y
        {
            get { return mY; }
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
            set { }
            get { return mText.Cols; }
        }

        /// <summary>
        /// Get window height.
        /// </summary>
        public int Rows
        {
            set { }
            get { return mText.Rows; }
        }

        /// <summary>
        /// Text screen.
        /// </summary>
        public HAL.TextScreenBase mText;

        /// <summary>
        /// Console object constructor.
        /// </summary>
        /// <param name="textScreen">Output device.</param>
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
        /// Clear console and return cursor to (0,0).
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
        /// Scroll the console up and move crusor to the start of the line.
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
        /// Move cursor to the start of the line.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn()
        {
            mX = 0;
            UpdateCursor();
        }

        /// <summary>
        /// Print tab to the console.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
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
        /// Write byte array to the console.
        /// </summary>
        /// <param name="aText">A byte array to write to the console.</param>
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
        /// Get and set console foreground color.
        /// </summary>
        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)(mText.GetColor() ^ (byte)((byte)Background << 4)); }
            set { mText.SetColors(value, Background); }
        }

        /// <summary>
        /// Get and set console background color.
        /// </summary>
        public ConsoleColor Background
        {
            get { return (ConsoleColor)(mText.GetColor() >> 4); }
            set { mText.SetColors(Foreground, value); }
        }

        /// <summary>
        /// Get and set cursor size.
        /// The value is percentage in the range 1-100.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to set value out of range.</exception>
        public int CursorSize
        {
            get { return mText.GetCursorSize(); }
            set
            {
                // Value should be a percentage from [1, 100].
                if (value < 1 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("value", value, "CursorSize value " + value + " out of range (1 - 100)");
                }

                mText.SetCursorSize(value);
            }
        }

        /// <summary>
        /// Get and set cursor visiblty.
        /// </summary>
        public bool CursorVisible
        {
            get { return mText.GetCursorVisible(); }
            set { mText.SetCursorVisible(value); }
        }
    }
}
