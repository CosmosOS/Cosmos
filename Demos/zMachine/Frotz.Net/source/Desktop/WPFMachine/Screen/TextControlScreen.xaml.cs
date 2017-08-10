using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Globalization;

using System.IO;
using Frotz;
using Frotz.Blorb;

using Frotz.Screen;
using Frotz.Constants;

using WPFMachine.Support;

namespace WPFMachine.Screen
{
    /// <summary>
    /// Interaction logic for WPFScreen.xaml
    /// </summary>
    public partial class TextControlScreen : UserControl, IZScreen, ZMachineScreen
    {
        private Window _parent;

        public TextControlScreen(Window Parent)
        {
            InitializeComponent();

            _parent = Parent;
            this.Margin = new Thickness(0);

            _parent = Parent;

            _cursorCanvas = new System.Windows.Controls.Canvas();
            _cursorCanvas.Background = ZColorCheck.ZColorToBrush(1, ColorType.Foreground);
            _cursorCanvas.Visibility = System.Windows.Visibility.Hidden;
            cnvsTop.Children.Add(_cursorCanvas);

            _sound = new FrotzSound();
            LayoutRoot.Children.Add(_sound);

            fColor = 1;
            bColor = 1;

            this.Background = ZColorCheck.ZColorToBrush(1, ColorType.Background);

            _substituion = new NumberSubstitution();

            setFontInfo();
        }

        public void AddInput(char InputKeyPressed)
        {
            OnKeyPressed(InputKeyPressed);
        }

        public event EventHandler<ZKeyPressEventArgs> KeyPressed;
        protected void OnKeyPressed(char key)
        {
            if (KeyPressed != null) KeyPressed(this, new ZKeyPressEventArgs(key));
        }

        public event EventHandler<GameSelectedEventArgs> GameSelected;

        NumberSubstitution _substituion;

        ScreenLines _regularLines = null;
        ScreenLines _fixedWidthLines = null;

        FrotzSound _sound;

        int cursorHeight = 2;

        System.Windows.Controls.Canvas _cursorCanvas;

        double charHeight = 1;
        double charWidth = 1;

        Size ActualCharSize = Size.Empty;

        int lines = 0;
        int chars = 0; /* in fixed font */

        ScreenMetrics _metrics;
        public ScreenMetrics Metrics { get { return _metrics; } }

        int _x = 0;
        int _y = 0;

        int _cursorX = 0;
        int _cursorY = 0;

        int fColor;
        int bColor;

        int scale = 1;

        FontInfo _regularFont;
        FontInfo _fixedFont;

        // TODO It might be easier to just grab the h/w in the funciton
        // TODO Find a way to hook this to an event
        public void SetCharsAndLines()
        {
            double height = this.ActualHeight;
            double width = this.ActualWidth;

            FormattedText fixedFt = buildFormattedText("A", _fixedFont, true, null, null);
            FormattedText propFt = buildFormattedText("A", _regularFont, true, null, null);

            double w = fixedFt.Width;
            double h = fixedFt.Height;

            charHeight = Math.Max(fixedFt.Height, propFt.Height);
            charWidth = fixedFt.Width;

            // Account for the margin of the Rich Text Box
            // TODO Find a way to determine what this should be, or to remove the margin
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
                    // scale = 2; // Ok, so the rest of things are at the right scale, but we've pulled back the images to 1x

                    screenWidth = os_._blorbFile.StandardSize.Width * scale;
                    screenHeight = os_._blorbFile.StandardSize.Height * scale;
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

            Conversion.Metrics = _metrics;

            _regularLines = new ScreenLines(_metrics.Rows, _metrics.Columns);
            _fixedWidthLines = new ScreenLines(_metrics.Rows, _metrics.Columns);

            _cursorCanvas.MinHeight = 2;
            _cursorCanvas.MinWidth = charWidth;

            ztc.SetMetrics(_metrics);
        }

        public void setFontInfo()
        {
            // TODO Should see if this can be moved into the ZTextControl
            ztc.SetFontInfo();

            int font_size = Properties.Settings.Default.FontSize;

            _regularFont = new FontInfo(Properties.Settings.Default.ProportionalFont, font_size);
            _fixedFont = new FontInfo(Properties.Settings.Default.FixedWidthFont, font_size);
        }

        public void HandleFatalError(string Message)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                // TODO I'd like this to reference the root window for modality
                MessageBox.Show(_parent, Message, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }));

            throw new ZMachineException(Message);
        }

        public ScreenMetrics GetScreenMetrics()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                SetCharsAndLines();

            }));

            return _metrics;
        }

        int lastX;
        int lastY;

        public void DisplayChar(char c)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ztc.AddDisplayChar(c);
            }));

            if (_inInputMode)
            {
                lastX = _x;
                lastY = _y;
            }
        }

        public void SetCursorPosition(int x, int y)
        {
            int prevY = _cursorY;

            _x = x.tcw();
            _y = y.tch();

            _cursorX = x;
            _cursorY = y;

            ztc.SetCursorPosition(x, y);
        }

        public void ScrollLines(int top, int height, int numlines)
        {
            ztc.ScrollLines(top, height, numlines);
        }

        public void ScrollArea(int top, int bottom, int left, int right, int units)
        {
            throw new Exception("Need to handle ScrollArea");

            // TODO If I scroll an area, need to move the graphics along with it
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            _cursorCanvas.SetValue(Canvas.TopProperty, (double)_cursorY + (charHeight - cursorHeight));
            _cursorCanvas.SetValue(Canvas.LeftProperty, (double)_cursorX);
        }

        private FormattedText buildFormattedText(String Text, FontInfo Font, bool UseDisplayMode,
            List<FontChanges> changes, DrawingContext dc)
        {
            TextFormattingMode tfm = TextFormattingMode.Display;
            FormattedText ft = new FormattedText(Text,
                   CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight,
                   Font.Typeface,
                   Font.PointSize,
                   ZColorCheck.ZColorToBrush(fColor, ColorType.Foreground),
                   _substituion, tfm);

            if (changes != null)
            {
                foreach (FontChanges fc in changes)
                {
                    setStyle(fc.FandS, fc, ft, dc);
                }
            }

            return ft;
        }

        public void setStyle(CharDisplayInfo fs, FontChanges fc, FormattedText ft, DrawingContext dc)
        {
            int startPos = fc.Offset + fc.StartCol;

            if ((fs.Style & (int)ZStyles.BOLDFACE_STYLE) > 0)
            {
                ft.SetFontWeight(FontWeights.Bold, startPos, fc.Count);
            }

            int rectColor = -1;
            ColorType type = ColorType.Foreground;

            if ((fs.Style & (int)ZStyles.REVERSE_STYLE) > 0)
            {
                ft.SetFontWeight(FontWeights.Bold, startPos, fc.Count);
                ft.SetForegroundBrush(ZColorCheck.ZColorToBrush(fs.BackgroundColor, ColorType.Background), startPos, fc.Count);

                rectColor = fs.ForegroundColor;
            }
            else
            {
                ft.SetForegroundBrush(ZColorCheck.ZColorToBrush(fs.ForegroundColor, ColorType.Foreground), startPos, fc.Count);
                if (fs.BackgroundColor > 1 && fs.BackgroundColor != bColor)
                {
                    rectColor = fs.BackgroundColor;
                    type = ColorType.Background;
                }
            }

            if ((fs.Style & (int)ZStyles.EMPHASIS_STYLE) > 0)
            {
                ft.SetFontStyle(FontStyles.Italic, startPos, fc.Count);
            }

            if ((fs.Style & (int)ZStyles.FIXED_WIDTH_STYLE) > 0)
            {
                ft.SetFontFamily(_fixedFont.Family, startPos, fc.Count);
            }

            if (dc != null && rectColor != -1)
            {
                Brush b = ZColorCheck.ZColorToBrush(rectColor, type);

                dc.DrawRectangle(b, null,
                    new Rect(fc.StartCol * charWidth, fc.Line * charHeight,
                        fc.Count * charWidth, charHeight));
            }
        }


        public void RefreshScreen()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                this.InvalidateVisual();

                ztc.Flush();
                ztc.Refresh();
            }));
        }

        public void SetTextStyle(int new_style)
        {
            ztc.SetTextStyle(new_style);
        }

        public void Clear()
        {
            ztc.Clear();

            Dispatcher.Invoke(new Action(delegate
            {
                this.Background = ZColorCheck.ZColorToBrush(bColor, ColorType.Background);

                for (int i = 0; i < cnvsTop.Children.Count; i++)
                {
                    {
                        Image img = cnvsTop.Children[i] as Image;
                        if (img != null)
                        {
                            cnvsTop.Children.RemoveAt(i--);
                        }
                    }

                }
            }));


        }

        public void ClearArea(int top, int left, int bottom, int right)
        {
            if (top == 1 && left == 1 && bottom == _metrics.WindowSize.Height && right == _metrics.WindowSize.Width)
            {
                Clear();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Clear area:" + top + ":" + left + ":" + bottom + ":" + right);
            }
        }

        public String OpenExistingFile(String defaultName, String Title, String Filter)
        {
            String name = null;
            Dispatcher.Invoke(new Action(delegate
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Title = Title;
                ofd.Filter = CreateFilterList(Filter);
                ofd.DefaultExt = ".sav";
                ofd.FileName = defaultName;
                if (ofd.ShowDialog(_parent) == true)
                {
                    name = ofd.FileName;
                }
                _parent.Focus(); // HACK For some reason, it won't always pick up text input after the dialog, so this refocuses
            }));
            return name;
        }

        public String OpenNewOrExistingFile(String defaultName, String Title, String Filter, String DefaultExtension)
        {
            String name = null;
            Dispatcher.Invoke(new Action(delegate
            {
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.Title = "Choose save game name";
                sfd.FileName = defaultName;

                sfd.Filter = CreateFilterList("Save Files (*.sav)|*.sav");
                sfd.DefaultExt = ".sav";

                if (sfd.ShowDialog(_parent) == true)
                {
                    name = sfd.FileName;
                }

                _parent.Focus(); // HACK For some reason, it won't always pick up text input after the dialog, so this refocuses
            }));
            return name;
        }


        public ZSize GetImageInfo(byte[] Image)
        {
            System.Drawing.Image i = System.Drawing.Image.FromStream(new MemoryStream(Image));

            return new ZSize(i.Height * scale, i.Width * scale);
        }


        public void DrawPicture(int picture, byte[] Image, int y, int x)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                Image img = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new System.IO.MemoryStream(Image);
                bi.EndInit();
                img.Source = bi;
                cnvsTop.Children.Add(img);

                int newX = x;
                int newY = y;
                // TODO Ok, so when calculating the position of the graphics, it's causing a wrap
                // TODO Find out why, and fix it...

                //
                if (newY > short.MaxValue) newY -= ushort.MaxValue;
                if (newX > short.MaxValue) newX -= ushort.MaxValue;

                img.SetValue(Canvas.TopProperty, (double)newY);
                img.SetValue(Canvas.LeftProperty, (double)newX);
            }));
        }

        public void SetFont(int font)
        {
            ztc.SetFont(font);
        }

        public void DisplayMessage(string Message, string Caption)
        {
            MessageBox.Show(Message, Caption);
        }

        public int GetStringWidth(string s, CharDisplayInfo Font)
        {
            FormattedText ft;
            if (Font.Font == ZFont.FIXED_WIDTH_FONT)
            {
                ft = buildFormattedText(s, _fixedFont, true, null, null);
            }
            else
            {
                ft = buildFormattedText(s, _regularFont, true, null, null);
            }

            return (int)ft.WidthIncludingTrailingWhitespace;
        }

        public bool GetFontData(int font, ref ushort height, ref ushort width)
        {
            switch (font)
            {
                case ZFont.TEXT_FONT:
                case ZFont.FIXED_WIDTH_FONT:
                    height = (ushort)_metrics.FontSize.Height;
                    width = (ushort)_metrics.FontSize.Width;
                    return true;
                case ZFont.GRAPHICS_FONT:
                case ZFont.PICTURE_FONT:
                    return false;
            }

            return false;
        }

        public void SetColor(int new_foreground, int new_background)
        {
            fColor = new_foreground;
            bColor = new_background;

            // ZColorCheck.setDefaults(new_foreground, new_background);
            ztc.SetColor(new_foreground, new_background);
        }

        public void RemoveChars(int count)
        {

            Dispatcher.Invoke(new Action(delegate
            {
                if (_inInputMode)
                {
                    ztc.RemoveInputChars(count);
                }
                else
                {
                    ztc.RemoveInputChars(count);
                    // HandleFatalError("Need to handle case where RemoveChars is called outside of input mode");
                }
            }));
        }

        public void addInputChar(char c)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ztc.AddInputChar(c);
            }));
        }

        public ushort PeekColor()
        {
            var f = _regularLines.GetFontAndStyle(_x, _y);
            return (ushort)f.BackgroundColor;
        }


        public void GetColor(out int foreground, out int background)
        {
            foreground = fColor;
            background = bColor;
        }

        public void PrepareSample(int number)
        {
            if (os_._blorbFile != null)
            {
                _sound.LoadSound(os_._blorbFile.Sounds[number]);
            }
        }

        public void StartSample(int number, int volume, int repeats, ushort eos)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                _sound.LoadSound(os_._blorbFile.Sounds[number]);
                _sound.PlaySound();
            }));
        }

        public void FinishWithSample(int number)
        {
        }

        public void StopSample(int number)
        {
        }

        public void SetInputColor()
        {
            // 32 means use the default input color
            SetColor(32, bColor);
        }

        private String CreateFilterList(params String[] types)
        {
            List<String> temp = new List<string>(types);
            temp.Add("All Files (*.*)|*.*");

            return String.Join("|", temp.ToArray());
        }

        private bool _inInputMode = false;
        public void SetInputMode(bool InputMode, bool CursorVisibility)
        {
            if (_inInputMode != InputMode)
            {
                _inInputMode = InputMode;

                Dispatcher.Invoke(new Action(delegate
                {
                    if (_inInputMode == true)
                    {
                        int x = ztc.StartInputMode();
                        if (_cursorX <= 1 && x > -1)
                        {
                            _cursorX = x + 2; // Move the cursor over 2 pixels to account for margin
                            _cursorCanvas.SetValue(Canvas.LeftProperty, (double)_cursorX);
                        }
                    }
                    else
                    {
                        ztc.EndInputMode();
                    }

                    if (CursorVisibility)
                    {
                        _cursorCanvas.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        _cursorCanvas.Visibility = System.Windows.Visibility.Hidden;
                    }
                }));
            }
        }


        public void StoryStarted(string StoryFileName, Blorb BlorbFile)
        {
            Dispatcher.Invoke(new Action(delegate
            {

                if (os_._blorbFile != null)
                {
                    _parent.Title = String.Format("FrotzNET - {0}", os_._blorbFile.StoryName);
                }
                else
                {
                    _parent.Title = String.Format("FrotzNET - {0}", StoryFileName);
                }

                if (GameSelected != null) GameSelected(this, new GameSelectedEventArgs(StoryFileName, BlorbFile));
            }));
        }


        public ZPoint GetCursorPosition()
        {
            ZPoint p = null;
            Dispatcher.Invoke(new Action(delegate
            {
                p = new ZPoint(_cursorX, _cursorY);
            }));
            return p;
        }

        public new void Focus()
        {
            base.Focus();
        }

        public string SelectGameFile(out byte[] filedata)
        {
            byte[] buffer = null;
            String fName = null;
            Dispatcher.Invoke(new Action(delegate
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Title = "Open a Z-Code file";
                ofd.DefaultExt = ".dat";

                ofd.Filter = CreateFilterList(
                 "Infocom Blorb File (*.zblorb)|*.zblorb",
                 "Infocom Games (*.dat)|*.dat",
                 "Z-Code Files (*.z?)|*.z?",
                 "Blorb File (*.blorb)|*.blorb");


                if (ofd.ShowDialog(_parent) == true)
                {
                    fName = ofd.FileName;
                    var s = ofd.OpenFile();
                    buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);
                    s.Close();
                }

            }));
            filedata = buffer;
            return fName;
        }


        public void Reset()
        {
            Clear();
        }


        public void SetActiveWindow(int win)
        {
        }

        public void SetWindowSize(int win, int top, int left,  int height, int width)
        {
        }

        void ZMachineScreen.AddInput(char InputKeyPressed)
        {
            throw new NotImplementedException();
        }

        void ZMachineScreen.SetCharsAndLines()
        {
            throw new NotImplementedException();
        }

        ScreenMetrics ZMachineScreen.Metrics
        {
            get { throw new NotImplementedException(); }
        }

        void ZMachineScreen.setFontInfo()
        {
            throw new NotImplementedException();
        }

        void ZMachineScreen.Focus()
        {
            throw new NotImplementedException();
        }

        event EventHandler<GameSelectedEventArgs> ZMachineScreen.GameSelected
        {
            add { throw new NotImplementedException(); }
            remove { throw new NotImplementedException(); }
        }

        public bool ShouldWrap()
        {
            return true;
        }

    }
}
