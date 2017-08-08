using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

using System.Globalization;

namespace WPFMachine.RTBSubclasses
{
    public class ZRun : Run
    {
        public ZRun(FontFamily Family)
            : this(Family, "")
        { }

        public ZRun(FontFamily Family, String Text)
            : base(Text)
        {
            this.FontStyle = System.Windows.FontStyles.Normal;
            this.FontWeight = System.Windows.FontWeights.Normal;
            this.FontFamily = Family;

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

        internal event EventHandler WidthChanged;
        protected void OnWidthChanged()
        {
            _width = null;
            if (WidthChanged != null)
            {
                WidthChanged(this, EventArgs.Empty);
            }
        }

        public new FontStyle FontStyle
        {
            get { return base.FontStyle; }
            set {
                if (base.FontStyle != value)
                {
                    OnWidthChanged();
                    base.FontStyle = value;
                }
            }
        }

        public new FontWeight FontWeight
        {
            get { return base.FontWeight; }
            set {
                if (base.FontWeight != value)
                {
                    OnWidthChanged();
                    base.FontWeight = value;
                }
            }
        }

        public new String Text
        {
            get { return base.Text; }
            set {
                if (base.Text != value)
                {
                    OnWidthChanged();
                    base.Text = value;
                }
            }
        }

        private double DetermineWidth()
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
