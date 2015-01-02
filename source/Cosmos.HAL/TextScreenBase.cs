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

        public abstract ushort Cols
        {
            get;
        }

        public abstract ushort Rows
        {
            get;
        }

        public abstract void SetCursorPos(int x, int y);

        public abstract void ScrollUp();

        public abstract char this[int x, int y]
        {
            get;
            set;
        }
    }
}