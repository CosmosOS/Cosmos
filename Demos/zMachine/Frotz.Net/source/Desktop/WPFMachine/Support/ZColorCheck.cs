using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using Frotz;
using Frotz.Constants;

using WPFMachine.Support;

using FrotzNet.Frotz.Other;

namespace WPFMachine
{
    public class ZColorCheck
    {
        static Color c64Blue = Color.FromRgb(66, 66, 231);

        private int _color;
        public int ColorCode { get { return _color; } set { _color = value; } }

        private ColorType _type;
        public ColorType Type { get { return _type; } set { _type = value; } }


        public ZColorCheck(int Color, ColorType Type)
        {
            _color = Color;
            _type = Type;
        }

        public bool AreSameColor(ZColorCheck ColorToCompare)
        {
            if (ColorToCompare == null) return false;

            if (ColorToCompare.ColorCode == 0 || _color == 0 && _type == ColorToCompare.Type) return true;

            return (ColorToCompare.ColorCode == _color && ColorToCompare.Type == _type);
        }

        internal Brush ToBrush()
        {
            return ZColorToBrush(_color, _type);
        }

        internal Color ToColor()
        {
            return ZColorToColor(_color, _type);
        }

        static ZColorCheck()
        {
            resetDefaults();
        }

        internal static void resetDefaults() {
            CurrentForeColor = Properties.Settings.Default.DefaultForeColor;
            CurrentBackColor = Properties.Settings.Default.DefaultBackColor;
        }

        internal static void setDefaults(int fore_color, int back_color)
        {
            if (fore_color > 1)
            {
                CurrentForeColor = ZColorToColor(fore_color, ColorType.Foreground);
            }

            if (back_color > 1)
            {
                CurrentBackColor = ZColorToColor(back_color, ColorType.Background);
            }
        }

        internal static Color CurrentForeColor { get; set; }
        internal static Color CurrentBackColor { get; set; }

        internal static Brush ZColorToBrush(int color, ColorType Type)
        {
            return new SolidColorBrush(ZColorToColor(color, Type));
        }

        internal static Color ZColorToColor(int color, ColorType Type)
        {
            if (color == 0 || color == 1)
            {
                if (Type == ColorType.Foreground) return CurrentForeColor;
                if (Type == ColorType.Background) return CurrentBackColor; 
            }

            switch (color)
            {
                case ZColor.BLACK_COLOUR:
                    return Colors.Black;
                case ZColor.BLUE_COLOUR:
                    return c64Blue;
                case ZColor.CYAN_COLOUR:
                    return Colors.Cyan;
                case ZColor.DARKGREY_COLOUR:
                    return Colors.DarkGray;
                case ZColor.GREEN_COLOUR:
                    return Colors.Green;
                // case ZColor.LIGHTGREY_COLOUR: // Light Grey & Grey both equal 10
                case ZColor.GREY_COLOUR:
                    return Colors.Gray;
                case ZColor.MAGENTA_COLOUR:
                    return Colors.Magenta;
                case ZColor.MEDIUMGREY_COLOUR:
                    return Colors.DimGray;
                case ZColor.RED_COLOUR:
                    return Colors.Red;
                case ZColor.TRANSPARENT_COLOUR:
                    return Colors.Transparent;
                case ZColor.WHITE_COLOUR:
                    return Colors.White;
                case ZColor.YELLOW_COLOUR:
                    return Colors.Yellow;
                case 32:
                    return Properties.Settings.Default.DefaultInputColor;
            }

            long new_color = TrueColorStuff.GetColour(color);
            byte r = TrueColorStuff.GetRValue(new_color);
            byte g = TrueColorStuff.GetGValue(new_color);
            byte b = TrueColorStuff.GetBValue(new_color);

            return Color.FromRgb(r, g, b);
        }
    }
}
