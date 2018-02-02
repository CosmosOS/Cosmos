using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL
{
    public abstract class TextScreenBase : Device
    {
        public abstract void Clear();

        public abstract void SetColors(ConsoleColor aForeground, ConsoleColor aBackground);

        public abstract byte GetColor();

        public abstract ushort Cols
        {
            get;
        }

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

        public abstract void SetCursorPos(int x, int y);

        public abstract void ScrollUp();

        public abstract byte this[int x, int y]
        {
            get;
            set;
        }

        public abstract int GetCursorSize();
        public abstract void SetCursorSize(int value);

        public abstract bool GetCursorVisible();
        public abstract void SetCursorVisible(bool value);

    }
}
