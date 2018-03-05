using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System {
    public class Console {
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private const byte Tab = (byte)'\t';
        private const byte Space = (byte)' ';

        protected int mX = 0;
        public int X {
            get { return mX; }
            set {
                mX = value;
                UpdateCursor();
            }
        }

        protected int mY = 0;
        public int Y {
            get { return mY; }
            set {
                mY = value;
                UpdateCursor();
            }
        }

        public int Cols {
            get { return mText.Cols; }
        }

        public int Rows {
            get { return mText.Rows; }
        }

        protected HAL.TextScreenBase mText;

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

        public void Clear() {
            mText.Clear();
            mX = 0;
            mY = 0;
            UpdateCursor();
        }

        //TODO: This is slow, batch it and only do it at end of updates
        protected void UpdateCursor() {
            mText.SetCursorPos(mX, mY);
        }

        private void DoLineFeed() {
            mY++;
            mX = 0;
            if (mY == mText.Rows) {
                mText.ScrollUp();
                mY--;
            }
            UpdateCursor();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoCarriageReturn() {
            mX = 0;
            UpdateCursor();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DoTab()
        {
            Write(Space);
            Write(Space);
            Write(Space);
            Write(Space);
        }

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

        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)(mText.GetColor() ^ (byte)((byte)Background << 4)); }
            set { mText.SetColors(value, Background); }
        }
        public ConsoleColor Background
        {
            get { return (ConsoleColor)(mText.GetColor() >> 4); }
            set { mText.SetColors(Foreground, value); }
        }

        public int CursorSize
        {
            get { return mText.GetCursorSize(); }
            set {
                // Value should be a percentage from [1, 100].
                if (value < 1 || value > 100)
                    throw new ArgumentOutOfRangeException("value", value, "CursorSize value " + value + " out of range (1 - 100)");

                mText.SetCursorSize(value);
            }
        }

        public bool CursorVisible {
            get { return mText.GetCursorVisible(); }
            set { mText.SetCursorVisible(value);  }
        }
    }
}
