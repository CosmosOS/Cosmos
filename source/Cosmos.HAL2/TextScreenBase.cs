using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL
{
    public abstract class TextScreenBase : Device
    {
        /// <summary>
        /// Clear text screen.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Get and set console foreground color.
        /// </summary>
        public abstract void SetColors(ConsoleColor aForeground, ConsoleColor aBackground);

        /// <summary>
        /// Get screen color.
        /// </summary>
        /// <returns>byte value.</returns>
        public abstract byte GetColor();

        /// <summary>
        /// Get number of columns in text screen.
        /// </summary>
        public abstract ushort Cols
        {
            get;
        }

        /// <summary>
        /// Get number of rows in text screen.
        /// </summary>
        public abstract ushort Rows
        {
            get;
        }

        public ConsoleColor Foreground
        {
            get { return (ConsoleColor)(GetColor() ^ (byte)((byte)Background << 4)); }
            set { SetColors(value, Background); }
        }
        public ConsoleColor Background
        {
            get { return (ConsoleColor)(GetColor() >> 4); }
            set { SetColors(Foreground, value); }
        }

        /// <summary>
        /// Set cursor position.
        /// </summary>
        /// <param name="x">A position on X axis.</param>
        /// <param name="y">A position on Y axis.</param>
        public abstract void SetCursorPos(int x, int y);

        /// <summary>
        /// Scroll screen up.
        /// </summary>
        public abstract void ScrollUp();

        public abstract byte this[int x, int y]
        {
            get;
            set;
        }

        /// <summary>
        /// Get cursor size.
        /// </summary>
        /// <returns>int value.</returns>
        public abstract int GetCursorSize();
        public abstract void SetCursorSize(int value);

        public abstract bool GetCursorVisible();
        public abstract void SetCursorVisible(bool value);

    }
}
