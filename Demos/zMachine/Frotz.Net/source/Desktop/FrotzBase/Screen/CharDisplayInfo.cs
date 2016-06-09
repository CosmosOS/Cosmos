using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Screen
{
    public struct CharDisplayInfo
    {
        private int _font;
        public int Font
        {
            get { return _font; }
            set { _font = value; }
        }

        private int _style;
        public int Style
        {
            get { return _style; }
            set { _style = value; }
        }

        private int _backgroundColor;
        public int BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        private int _foregroundColor;
        public int ForegroundColor
        {
            get { return _foregroundColor; }
            set { _foregroundColor = value; }
        }

        public CharDisplayInfo(int Font, int Style, int BackgroundColor, int ForegroundColor)
        {
            _font = Font;
            _style = Style;
            _backgroundColor = BackgroundColor;
            _foregroundColor = ForegroundColor;
        }

        public override string ToString()
        {
            return String.Format("Font: {0} Style: {1}: Fore: {2} Back: {3}", Font, Style, ForegroundColor, BackgroundColor);
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException("Need to find this");
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool AreSame(CharDisplayInfo fs)
        {
            return (fs.Font == this.Font && fs.Style == this.Style && fs.ForegroundColor == this.ForegroundColor && fs.BackgroundColor == this.BackgroundColor);
        }

        public static CharDisplayInfo Empty
        {
            get { return new CharDisplayInfo(0, 0, 0, 0); }
        }

        public bool ImplementsStyle(int StyleBit)
        {
            return ((this.Style & StyleBit) > 0);
        }
    }
}
