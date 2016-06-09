using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;

using Frotz.Screen;
using Frotz;
using System.Windows.Media;
using Frotz.Constants;
using WPFMachine.Support;
using System.Globalization;

namespace WPFMachine
{
    public abstract class ScreenBase : UserControl, ZMachineScreen
    {
        #region ZMachineScreen Members

        public void AddInput(char InputKeyPressed)
        {
            OnKeyPressed(InputKeyPressed);
        }

        protected void OnKeyPressed(char key)
        {
            if (KeyPressed != null) KeyPressed(this, new ZKeyPressEventArgs(key));
        }

        public event EventHandler<ZKeyPressEventArgs> KeyPressed;

        protected int scale = 1;

        protected double charWidth = 1; // TODO This should probably be an int
        protected double charHeight = 1; // TODO Same here

        protected ScreenMetrics _metrics;

        public ScreenMetrics Metrics { get { return _metrics; } }

        protected FontInfo _regularFont;
        protected FontInfo _fixedFont;
        protected FontInfo _beyZorkFont;

        protected CharDisplayInfo _currentInfo;

        protected Window _parent;

        protected Size ActualCharSize = Size.Empty;
        protected int lines = 0;
        protected int chars = 0; /* in fixed font */

        protected ScreenLines _regularLines = null; // TODO Are these even used now?
        protected ScreenLines _fixedWidthLines = null;

        protected NumberSubstitution _substituion = new NumberSubstitution();
        
        public void SetCharsAndLines()
        {
            double height = this.ActualHeight;
            double width = this.ActualWidth;

            FormattedText fixedFt = buildFormattedText("A", _fixedFont, _currentInfo, null);
            FormattedText propFt = buildFormattedText("A", _regularFont, _currentInfo, null);

            double w = fixedFt.Width;
            double h = fixedFt.Height;

            charHeight = Math.Max(fixedFt.Height, propFt.Height);
            charWidth = fixedFt.Width;

            double screenWidth = width - 20;
            double screenHeight = height - 20;

            if (os_._blorbFile != null)
            {
                var standard = os_._blorbFile.StandardSize;
                if (standard.Height > 0 && standard.Width > 0)
                {
                    int maxW = (int)Math.Floor(width / os_._blorbFile.StandardSize.Width);
                    int maxH = (int)Math.Floor(height / os_._blorbFile.StandardSize.Height);

                    scale = Math.Min(maxW, maxH);

                    screenWidth = os_._blorbFile.StandardSize.Width * scale;
                    screenHeight = os_._blorbFile.StandardSize.Height * scale;

                    double heightDiff = _parent.ActualHeight - this.ActualHeight;
                    double widthDiff = _parent.ActualWidth - this.ActualWidth;

                    _parent.Height = screenHeight + heightDiff;
                    _parent.Width = screenWidth + widthDiff;
                }
                else
                {
                    scale = 1;
                }
            }
            else
            {
                scale = 1;
            }

            ActualCharSize = new Size(propFt.Width, propFt.Height);

            chars = Convert.ToInt32(Math.Floor(screenWidth / charWidth)); // Determine chars based only on fixed width chars since proportional fonts are accounted for as they are written
            lines = Convert.ToInt32(Math.Floor(screenHeight / charHeight)); // Use the largest character height

            _metrics = new ScreenMetrics(
                new ZSize(charHeight, charWidth),// new ZSize(h, w),
                new ZSize(lines * charHeight, chars * charWidth), // The ZMachine wouldn't take screenHeight as round it down, so this takes care of that
                lines, chars, scale);

            _regularLines = new ScreenLines(_metrics.Rows, _metrics.Columns);
            _fixedWidthLines = new ScreenLines(_metrics.Rows, _metrics.Columns);

            AfterSetCharsAndLines();
        }

        protected abstract void AfterSetCharsAndLines();

        protected FormattedText buildFormattedText(String Text, FontInfo Font, CharDisplayInfo cdi, DrawingContext dc)
        {
            TextFormattingMode tfm = TextFormattingMode.Display;
            FormattedText ft = new FormattedText(Text,
                   CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight,
                   Font.Typeface,
                   Font.PointSize,
                   ZColorCheck.ZColorToBrush(cdi.ForegroundColor, ColorType.Foreground),
                   _substituion, tfm);

            setStyle(cdi, Text.Length, ft);

            return ft;
        }

        public void setStyle(CharDisplayInfo fs, int count, FormattedText ft)
        {
            if ((fs.Style & (int)ZStyles.BOLDFACE_STYLE) > 0)
            {
                ft.SetFontWeight(FontWeights.Bold);
            }

            if ((fs.Style & (int)ZStyles.REVERSE_STYLE) > 0)
            {
                ft.SetFontWeight(FontWeights.Bold);
                ft.SetForegroundBrush(ZColorCheck.ZColorToBrush(fs.BackgroundColor, ColorType.Background));
            }
            else
            {
                ft.SetForegroundBrush(ZColorCheck.ZColorToBrush(fs.ForegroundColor, ColorType.Foreground));
            }

            if ((fs.Style & (int)ZStyles.EMPHASIS_STYLE) > 0)
            {
                ft.SetFontStyle(FontStyles.Italic);
            }

            if ((fs.Style & (int)ZStyles.FIXED_WIDTH_STYLE) > 0)
            {
                ft.SetFontFamily(_fixedFont.Family);
            }
        }

        public void setFontInfo()
        {
            int font_size = Properties.Settings.Default.FontSize;

            _regularFont = new FontInfo(Properties.Settings.Default.ProportionalFont, font_size);
            _fixedFont = new FontInfo(Properties.Settings.Default.FixedWidthFont, font_size);
        }

        public new void Focus()
        {
            base.Focus(); // TODO Determine if this is actually necessary
        }

        public event EventHandler<GameSelectedEventArgs> GameSelected;

        public void Reset()
        {
            DoReset();
        }

        protected abstract void DoReset();

        protected void OnStoryStarted(GameSelectedEventArgs e)
        {
            if (GameSelected != null) GameSelected(this, e);
        }

        Dictionary<int, byte[]> images = new Dictionary<int, byte[]>();

        public ZSize GetImageInfo(byte[] Image)
        {
            System.Drawing.Image i = System.Drawing.Image.FromStream(new System.IO.MemoryStream(Image));

            return new ZSize(i.Height * scale, i.Width * scale);
        }

        // TODO This does the same thing (blurry) and doesn't retain the transparent
        public byte[] scaleImage(int scale, byte[] Image)
        {
            System.Drawing.Image i = System.Drawing.Image.FromStream(new System.IO.MemoryStream(Image));
            System.Drawing.Bitmap b = new System.Drawing.Bitmap(i.Width * scale, i.Height * scale);
            using (var g = System.Drawing.Graphics.FromImage((System.Drawing.Image)b))
            {
                g.DrawImage(i, 0, 0, i.Width * scale, i.Height * scale);
            }
            var ms = new System.IO.MemoryStream();

            b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            return ms.ToArray();
        }
           
        #endregion
    }
}
