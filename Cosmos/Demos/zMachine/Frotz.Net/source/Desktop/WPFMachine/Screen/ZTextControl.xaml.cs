using System;
using System.Collections.Generic;
using System.Linq;
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

using WPFMachine.Support;

using Frotz.Constants;
using Frotz.Screen;

namespace WPFMachine.Screen
{
    /// <summary>
    /// Interaction logic for NewerTextControl.xaml
    /// </summary>
    public partial class ZTextControl : UserControl
    {
        internal ScreenMetrics _metrics;
        internal FontInfo _regularFont;
        internal FontInfo _fixedFont;
        FlowDocument _fd;

        internal int fColor;
        internal int bColor;

        int _x = 1;
        int _y = 1;

        int _cursorX = 1;
        int _cursorY = 1;

        ZParagraph _currentParagraph;

        CharDisplayInfo _currentDisplay;

        internal OverlayAdorner _adorner;

        public ZTextControl()
        {
            InitializeComponent();

            _currentDisplay = new CharDisplayInfo(ZFont.TEXT_FONT, ZStyles.NORMAL_STYLE, 1, 1);
            _currentParagraph = new ZParagraph(this, _currentDisplay);


            _adorner = new OverlayAdorner(rtb);

            this.Loaded += new RoutedEventHandler(ZTextControl_Loaded);
        }

        void ZTextControl_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO This may be used to simplify some of the text handling
            var layer = AdornerLayer.GetAdornerLayer(rtb);
            layer.Add(_adorner);
        }

        internal void SetMetrics(ScreenMetrics Metrics)
        {
            _metrics = Metrics;

            _adorner.FontHeight = Metrics.FontSize.Height;

            addLines();
        }

        private void addLines()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                double top = (_fd.Blocks.Count * _metrics.FontSize.Height) + 1;
                while (_fd.Blocks.Count < _metrics.Rows)
                {
                    {
                        ZParagraph p = new ZParagraph(this, _currentDisplay);
                        p.LineHeight = _metrics.FontSize.Height;
                        _fd.Blocks.Add(p);

                    }
                }

                // Reset the top on all the lines to make sure they are correct
                double newTop = 0;
                foreach (ZParagraph zp in _fd.Blocks)
                {
                    zp.Top = newTop;
                    if (zp.Top == 0) zp.Top = 1;
                    newTop += _metrics.FontSize.Height;
                }

                SetCurrentParagraph();
            }));
        }

        internal void SetFontInfo()
        {
            int font_size = Properties.Settings.Default.FontSize;

            _regularFont = new FontInfo(Properties.Settings.Default.ProportionalFont, font_size);
            _fixedFont = new FontInfo(Properties.Settings.Default.FixedWidthFont, font_size);

            rtb.FontFamily = _regularFont.Family;
            rtb.FontSize = _regularFont.PointSize;
            _fd = new System.Windows.Documents.FlowDocument();
            rtb.Document = _fd;
            rtb.Background = Brushes.Transparent;
            rtb.Foreground = ZColorCheck.ZColorToBrush(1, ColorType.Foreground);
            rtb.IsReadOnly = true;

            _adorner.RegularFont = _regularFont;
            _adorner.FixedWidthFont = _fixedFont;
            _adorner.Clear();

            // this.Background = ZColorCheck.ZColorToBrush(1, ColorType.Background);
        }

        internal void AddDisplayChar(char c)
        {
            _currentParagraph.AddDisplayChar(c);
        }

        internal void SetCursorPosition(int x, int y)
        {
            _x = x.tcw();
            _y = y.tch();

            _cursorX = x;
            _cursorY = y;

            SetCurrentParagraph();
        }

        internal void ScrollLines(int top, int height, int numlines)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                _adorner.ScrollLines(top, height, numlines);

                _currentParagraph.Flush();
                _currentParagraph = null;
                // _cursorY = -1;

                List<ZParagraph> toRemove = new List<ZParagraph>();
                foreach (ZParagraph zp in _fd.Blocks)
                {
                    if (zp.Top >= top && zp.Top < top + numlines)
                    {
                        toRemove.Add(zp);
                    }
                }

                foreach (ZParagraph zp in toRemove)
                {
                    _fd.Blocks.Remove(zp);
                }

                addLines();

                double newTop = 1;
                foreach (ZParagraph zp in _fd.Blocks)
                {
                    zp.Top = newTop;
                    newTop += _metrics.FontSize.Height;
                }

            }));

            SetCurrentParagraph();

        }

        internal void Refresh()
        {
            _adorner.InvalidateVisual();
        }

        internal void Clear()
        {
            Dispatcher.Invoke(new Action(delegate
            {
                _adorner.Clear();

                // TODO Consider erasing the text in the existing lines rather than blowing out the whole thing
                _fd.Blocks.Clear();
                addLines();
            }));
        }

        public void SetTextStyle(int new_style)
        {
            if (_currentDisplay.Style != new_style)
            {
                _currentDisplay.Style = new_style;
                _currentParagraph.SetDisplayInfo(_currentDisplay);
            }
        }

        public void SetColor(int new_foreground, int new_background)
        {
            if (fColor != new_foreground || bColor != new_background)
            {
                _currentDisplay.ForegroundColor = new_foreground;
                _currentDisplay.BackgroundColor = new_background;
                
                // TODO Can this be removed?
                fColor = new_foreground;
                bColor = new_background;

                _currentParagraph.SetDisplayInfo(_currentDisplay);
            }
        }

        public void SetFont(int font)
        {
            if (_currentDisplay.Font != font)
            {
                _currentDisplay.Font = font;
                _currentParagraph.SetDisplayInfo(_currentDisplay);
            }
        }

        public void Flush()
        {
            _currentParagraph.Flush();
        }

        public int StartInputMode()
        {
            _currentParagraph.Flush();
            _currentParagraph.StartInputMode();

            int width = 0;

            foreach (Inline il in _currentParagraph.Inlines)
            {
                if (il is ZRun)
                {
                    ZRun r = il as ZRun;
                    String text = r.Text;
                    if (text.Contains(">"))
                    {
                        String temp = text.TrimEnd();
                        if (temp.LastIndexOf(">") == temp.Length - 1)
                        {
                            width += (int)r.Width;
                            return width;
                        }
                        else
                        {
                            // MessageBox.Show("> is not the last character in the line");
                            width += (int)r.Width;
                        }
                    }
                    else
                    {
                        width += (int)r.Width;
                    }
                }
                else if (il is ZBlankContainer)
                {
                    width += ((ZBlankContainer)il).Width;
                }
                else
                {
                    throw new Exception("Run isn't of ZRun or ZBlankContainer");
                }
            }

            return -1;
        }

        public void AddInputChar(char c)
        {
            _currentParagraph.AddInputChar(c);
        }

        public void RemoveInputChars(int count)
        {
            _currentParagraph.RemoveInputChars(count);
        }

        public void EndInputMode()
        {
            _currentParagraph.EndInputMode();
        }

        private void SetCurrentParagraph()
        {
            ZParagraph temp = null;
            Dispatcher.Invoke(new Action(delegate
            {

                foreach (ZParagraph zp in _fd.Blocks)
                {
                    // TODO This might be a bug somewhere
                    // For some reason, _cursorY is fluctuating by 1
                    if (Math.Abs(zp.Top - _cursorY) <= 1)
                    {
                        temp = zp;
                    }
                }

                if (_currentParagraph == temp)
                {
                    _currentParagraph.SetCursorXPosition(_cursorX);
                    return;
                }

                if (temp != null)
                {
                    if (_currentParagraph != null) _currentParagraph.Flush();

                    _currentParagraph = temp;
                    _currentParagraph.SetCursorXPosition(_cursorX);
                    _currentParagraph.SetDisplayInfo(_currentDisplay);
                }

            }));
            if (temp == null)
            {
                System.Diagnostics.Debug.WriteLine("Not matching an existing paragraph:" + _cursorY + ":" + _metrics.FontSize.Height + ":" + _metrics.WindowSize.Height);
            }
        }
    }
}
