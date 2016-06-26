using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Frotz;
using Frotz.Screen;
using System.Windows.Controls;

namespace WPFMachine {
    internal static class Conversion {
        internal static int tcw(this int w) {
            if (w == 1) { return 0; } else { return w / Metrics.FontSize.Width; }
        }

        internal static int tch(this int h) {
            if (h == 1) { return 0; } else { return h / Metrics.FontSize.Height; }
        }

        // TODO I won't need this
        internal static int tsw(this int w) {
            if (w == 1) { return 0; } else { return w; }
        }

        internal static int tsh(this int h) {
            if (h == 1) { return 0; } else { return h; }
        }

        // TODO FIgure out how to make this easier to call
        internal static int tch(this int h, int min) {

            return Math.Max(min, h / Metrics.FontSize.Height);
        }

        internal static ScreenMetrics Metrics { get; set; }

        internal static int Top(this System.Windows.Controls.Image img) {
            return Convert.ToInt32(img.GetValue(Canvas.TopProperty));
        }

        internal static int Left(this System.Windows.Controls.Image img) {
            return Convert.ToInt32(img.GetValue(Canvas.LeftProperty));
        }

        internal static int Right(this System.Windows.Controls.Image img) {
            return img.getNumValue(Canvas.RightProperty);
        }

        internal static int Bottom(this System.Windows.Controls.Image img) {
            return img.getNumValue(Canvas.BottomProperty);
        }

        private static int getNumValue(this System.Windows.Controls.Image img, System.Windows.DependencyProperty prop) {
            double val = (double)img.GetValue(prop);
            
            if (double.IsNaN(val)) return 1;
            else return Convert.ToInt32(val);
        }
    }
}
