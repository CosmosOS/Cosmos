using System;
using System.Collections.Generic;
using System.Text;

using Frotz.Constants;

namespace Frotz.Screen
{
    public class ZKeyPressEventArgs : EventArgs
    {
        public char KeyPressed { get; private set; }

        public ZKeyPressEventArgs(Char KeyPressed)
        {
            this.KeyPressed = KeyPressed;
        }
    }

    public class ZPoint
    {
        public int X;
        public int Y;

        public ZPoint(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    public class ZSize
    {
        public int Height;
        public int Width;

        private ZSize() { }

        public ZSize(int Height, int Width)
        {
            this.Height = Height;
            this.Width = Width;
        }

        public ZSize(double Height, double Width)
        {
            this.Height = Convert.ToInt32(System.Math.Ceiling(Height));
            this.Width = Convert.ToInt32(System.Math.Ceiling(Width));
        }

        public static ZSize Empty
        {
            get { return new ZSize(0, 0); }
        }
    }

    public class ScreenMetrics
    {
        public ZSize FontSize;
        public ZSize WindowSize;
        public int Rows;
        public int Columns;
        public int Scale { get; set; }

        public ScreenMetrics(ZSize FontSize, ZSize WindowSize, int Rows, int Columns, int Scale)
        {
            this.FontSize = FontSize;
            this.WindowSize = WindowSize;
            this.Rows = Rows;
            this.Columns = Columns;
            this.Scale = Scale;
        }
    }




    public class FontChanges
    {
        public int Offset { get; set; }
        public int StartCol { get; private set; }
        public int Count { get; set; }
        public int Style { get { return FandS.Style; } }
        public int Font { get { return FandS.Font; } }
        public String Text { get { return _sb.ToString(); } }
        public int Line { get; set; }

        public CharDisplayInfo FandS { get; set; }

        private StringBuilder _sb = new StringBuilder();

        internal void AddChar(char c)
        {
            _sb.Append(c);
        }

        public FontChanges(int StartCol, int Count, CharDisplayInfo FandS)
        {
            this.StartCol = StartCol;
            this.Count = Count;

            this.FandS = FandS;
        }
    }
}
