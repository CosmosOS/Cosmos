using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using Frotz.Constants;
using Frotz.Screen;

namespace WPFMachine.Screen
{
    internal class ZRun : Run
    {
        internal CharDisplayInfo DisplayInfo { get; private set; }

        internal ZRun(CharDisplayInfo DisplayInfo)
        {
            this.DisplayInfo = DisplayInfo;
        }

        private double? _width = null;
        public double Width
        {
            get
            {
                if (_width == null)
                {
                    _width = DetermineWidth();
                }
                return (double)_width;
            }
        }

        internal double DetermineWidth()
        {
            return DetermineWidth(this.Text);
        }

        internal double DetermineWidth(String Text)
        {
            NumberSubstitution ns = new NumberSubstitution();
            FormattedText ft = new FormattedText(Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch),
            this.FontSize,
            this.Foreground, ns, TextFormattingMode.Display);

            return ft.Width;
        }
    }
}
