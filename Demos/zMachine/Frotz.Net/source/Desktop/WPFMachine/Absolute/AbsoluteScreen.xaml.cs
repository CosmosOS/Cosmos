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
using System.Globalization;

using Frotz;
using Frotz.Screen;
using Frotz.Constants;
using Frotz.Blorb;
using System.Xml;

using WPFMachine;

namespace WPFMachine.Absolute
{
    /// <summary>
    /// Interaction logic for Screen2.xaml
    /// </summary>
    public partial class AbsoluteScreen : ScreenBase, IZScreen
    {
        StringBuilder _currentText = new StringBuilder();

        int _activeWindow = -1;

        ScrollbackArea _scrollback;
        public ScrollbackArea Scrollback
        {
            get { return _scrollback; }
        }


        public AbsoluteScreen(Window Parent)
        {
            InitializeComponent();

            _parent = Parent;

            _scrollback = new ScrollbackArea(this);

            _cursorCanvas = new System.Windows.Controls.Canvas();
            _cursorCanvas.Background = ZColorCheck.ZColorToBrush(1, ColorType.Foreground);
            _cursorCanvas.Visibility = System.Windows.Visibility.Visible;
            cnvsTop.Children.Add(_cursorCanvas);

            _sound = new FrotzSound();
            LayoutRoot.Children.Add(_sound);


            _substituion = new NumberSubstitution();

            setFontInfo();

            _currentInfo = new CharDisplayInfo(1, 0, 1, 1);
            bColor = 1;
            this.Background = ZColorCheck.ZColorToBrush(bColor, ColorType.Background);

            this.MouseDown += new MouseButtonEventHandler(AbsoluteScreen_MouseDown);
            this.MouseDoubleClick += new MouseButtonEventHandler(AbsoluteScreen_MouseDoubleClick);
        }

        void mouseMove(MouseButtonEventArgs e, ushort mouseEvent)
        {
            Point p = e.GetPosition(this);
            os_.mouse_moved((ushort)p.X, (ushort)p.Y);
            AddInput((char)mouseEvent);
        }
        
        void AbsoluteScreen_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mouseMove(e, Frotz.Constants.CharCodes.ZC_DOUBLE_CLICK);
        }

        void AbsoluteScreen_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseMove(e, Frotz.Constants.CharCodes.ZC_SINGLE_CLICK);
        }

        Dictionary<char, String> graphicsChars = new Dictionary<char, string>();

        public void DisplayChar(char c)
        {
            if (_currentInfo.Font == ZFont.GRAPHICS_FONT)
            {

                invoke(() =>
                {
#if !TEMP
                    String lines = null;
                    if (graphicsChars.ContainsKey(c))
                    {
                        lines = graphicsChars[c];
                    }
                    else
                    {
                        String temp = Frotz.Other.GraphicsFont.getLines(c);
                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < 8; i++)
                        {
                            int x = Convert.ToInt32(temp.Substring(i * 2, 2), 16);
                            for (int j = 0; j < 8; j++)
                            {
                                int toggled = (x >> j) & 1;
                                if (toggled == 1)
                                {
                                    //sb.AppendFormat("<Line X1=\"{0}\" Y1=\"{1}\" X2=\"{2}\" Y2=\"{3}\" Stroke=\"White\" StrokeThickness=\"1\" />\r\n",
                                    //    j, i, j + 1, i + 1);
                                    sb.AppendFormat("M {0} {1} L {2} {3} ",
                                        j, i, j + 1, i);
                                }
                            }
                        }
                        lines = String.Format(@"
    <Image xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" Width=""8"" Height=""8"" Stretch=""None"">
        <Image.Source>
            <DrawingImage>
                <DrawingImage.Drawing>
                    <GeometryDrawing Geometry=""{0} "">
                        <GeometryDrawing.Pen>
                            <Pen Brush=""White"" Thickness=""1"" />
                        </GeometryDrawing.Pen>
                    </GeometryDrawing>
                </DrawingImage.Drawing>
            </DrawingImage>
        </Image.Source>
    </Image>", sb.ToString());

                        sb.ToString();
                        graphicsChars.Add(c, lines);

                        //var sw = new System.IO.StreamWriter(String.Format(@"c:\temp\{0}.xaml", (byte)c));
                        //sw.Write(lines);
                        //sw.Close();
                    }

                    //String temp = String.Format("<Canvas xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">{0}</Canvas>", Frotz.Other.GraphicsFont.getLines(c));
                    //temp = temp.Replace("Black", "White");
                    Image img = System.Windows.Markup.XamlReader.Parse(lines) as Image;

                    Canvas cnvs = new Canvas();
                    cnvs.Children.Add(img);

                    img.SnapsToDevicePixels = true;
                    // img.Stretch = Stretch.Uniform;

                    cnvs.SetValue(Canvas.TopProperty, (double)_cursorY);
                    cnvs.SetValue(Canvas.LeftProperty, (double)_cursorX);
                    cnvs.SetValue(Canvas.RightProperty, (double)(_cursorX + _metrics.FontSize.Width));
                    cnvs.SetValue(Canvas.BottomProperty, (double)(_cursorY + _metrics.FontSize.Height));

                    _cursorX += _metrics.FontSize.Width;

                    mainCanvas.Children.Add(cnvs);


#else


                    var bmp = Frotz.Other.GraphicsFont.getImage(c);

                    var ms = new System.IO.MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                    bmp.Save("C:\\TEMP\\TEST.BMP");

                    ms.Position = 0;
                    Image img = new Image();
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = ms;
                    bi.EndInit();
                    img.Source = bi;

                    img.Stretch = Stretch.Fill;

                    img.SetValue(Canvas.TopProperty, (double)_cursorY);
                    img.SetValue(Canvas.LeftProperty, (double)_cursorX);
                    img.SetValue(Canvas.RightProperty, (double)(_cursorX + _metrics.FontSize.Width));
                    img.SetValue(Canvas.BottomProperty, (double)(_cursorY + _metrics.FontSize.Height));

                    // _cursorX += Convert.ToInt32(bi.Width);
                    _cursorX += _metrics.FontSize.Width;

                    mainCanvas.Children.Add(img);

                    //Image img = new Image();
                    //img.Source = null;

                    //BitmapImage bi = new BitmapImage();
                    // 

                    //Image img = new Image();
                    //BitmapImage bi = new BitmapImage();
                    //bi.BeginInit();
                    //bi.StreamSource = new System.IO.MemoryStream(ScaleImages.Scale(buffer, scale));
                    //bi.EndInit();
                    //img.Source = bi;

                    //int newX = x;
                    //int newY = y;

                    //if (newY > short.MaxValue) newY -= ushort.MaxValue;
                    //if (newX > short.MaxValue) newX -= ushort.MaxValue;

                    //img.SetValue(Canvas.TopProperty, (double)newY);
                    //img.SetValue(Canvas.LeftProperty, (double)newX);

                    //mainCanvas.Children.Add(img);
#endif
                });
            }
            else
            {
                _currentText.Append(c);
            }
        }

        public void RefreshScreen()
        {
            FlushCurrentString(); // TODO Determine if anything else needs to be done here
        }

        public void SetCursorPosition(int x, int y)
        {
            if (!inInputMode)
            {
                FlushCurrentString();

                if (_activeWindow == 0 && y != _cursorY)
                {
                    _scrollback.AddString("\r\n", _currentInfo);
                }
                _cursorX = x;
                _cursorY = y;

                lastDrawn = Rect.Empty;
            }

        }

        public void ScrollLines(int top, int height, int lines)
        {
            FlushCurrentString();

            _scrollback.AddString("\r\n", _currentInfo);

            invoke(() =>
            {
                for (int i = 0; i < mainCanvas.Children.Count; i++)
                {
                    var c = mainCanvas.Children[i];
                    Image img = c as Image;
                    if (img != null)
                    {
                        double iTop = (double)img.GetValue(Canvas.TopProperty);
                        double iLeft = (double)img.GetValue(Canvas.LeftProperty);

                        double iBottom = iTop + img.ActualHeight;
                        double iRight = iLeft + img.ActualWidth;

                        if (iTop >= top && iBottom <= top + height)
                        {
                            double newPos = iTop - lines;
                            if (newPos >= top)
                            {
                                img.SetValue(Canvas.TopProperty, newPos);
                            }
                            else
                            {
                                mainCanvas.Children.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            });
        }

        private void FlushCurrentString()
        {
            if (_currentText.Length == 0 || inInputMode) return;

            String text = _currentText.ToString();
            _currentText.Clear();

            if (_activeWindow == 0)
            {
                _scrollback.AddString(text, _currentInfo);
            }

            SendStringToScreen(text, _currentInfo);

        }

        Rect lastDrawn = Rect.Empty;

        private void SendStringToScreen(String text, CharDisplayInfo cdi)
        {

            invoke(() =>
            {
                Image myImage = new Image();

                DrawingVisual dv = new DrawingVisual();
                DrawingContext dc = dv.RenderOpen();

                double x = _cursorX;
                double y = _cursorY;

                if (lastDrawn != Rect.Empty && inInputMode == false)
                {
                    x = lastDrawn.X + lastDrawn.Width;
                }


                FontInfo fi = _regularFont;
                if (cdi.Font == 4)
                {
                    fi = _fixedFont;
                }

                FormattedText ft = buildFormattedText(text, fi, cdi, dc);

                Brush b = Brushes.Transparent;

                if (cdi.ImplementsStyle(ZStyles.REVERSE_STYLE))
                {
                    b = ZColorCheck.ZColorToBrush(cdi.ForegroundColor, ColorType.Foreground);
                }
                else
                {
                    if (_currentInfo.BackgroundColor != bColor)
                    {
                        b = ZColorCheck.ZColorToBrush(cdi.BackgroundColor, ColorType.Background);
                    }
                }
                dc.DrawRectangle(b, null, new Rect(0, 0, ft.WidthIncludingTrailingWhitespace, charHeight));

                dc.DrawText(ft, new Point(0, 0));
                dc.Close();

                RenderTargetBitmap bmp = new RenderTargetBitmap((int)dv.ContentBounds.Width, (int)charHeight, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(dv);

                myImage.Source = bmp;

                mainCanvas.Children.Add(myImage);
                myImage.SetValue(Canvas.TopProperty, y);
                myImage.SetValue(Canvas.LeftProperty, x);

                lastDrawn = new Rect(x, y, (int)dv.ContentBounds.Width, charHeight);

                removeCoveredImages(myImage);
            });
        }

        public void SetTextStyle(int new_style)
        {
            if (new_style != _currentInfo.Style)
            {
                FlushCurrentString();
                _currentInfo.Style = new_style;
            }
        }

        public void SetFont(int font)
        {
            if (_currentInfo.Font != font)
            {
                FlushCurrentString();
                _currentInfo.Font = font;
            }
        }

        private void invoke(Action a)
        {
            Dispatcher.Invoke(a);
        }

        public void Clear()
        {
            invoke(() =>
            {
                mainCanvas.Children.Clear();
                bColor = _currentInfo.BackgroundColor;
                this.Background = ZColorCheck.ZColorToBrush(_currentInfo.BackgroundColor, ColorType.Background);
            });
        }

        public void ClearArea(int top, int left, int bottom, int right)
        {
            _scrollback.AddString("\r\n", _currentInfo);
            
            Rect r = new Rect(left, top, right - left, bottom - top);

            invoke(() =>
            {
                for (int i = 0; i < mainCanvas.Children.Count; i++)
                {
                    var c = mainCanvas.Children[i];
                    Image img = c as Image;
                    if (img != null)
                    {
                        double iTop = (double)img.GetValue(Canvas.TopProperty);
                        double iLeft = (double)img.GetValue(Canvas.LeftProperty);

                        double iBottom = iTop + img.ActualHeight;
                        double iRight = iLeft + img.ActualWidth;

                        Rect iRect = new Rect(iLeft, iTop, iRight - iLeft, iBottom - iTop);
                        Point p = new Point(iLeft, iTop);

                        if (r.Contains(p))
                        {
                            mainCanvas.Children.RemoveAt(i);
                            i--;
                        }
                    }
                }
            });
        }

        public void ScrollArea(int top, int bottom, int left, int right, int units)
        {
            FlushCurrentString();

            _scrollback.AddString("\r\n", _currentInfo);

            Rect r = new Rect(left, top, right - left, bottom - top);

            invoke(() =>
            {
                for (int i = 0; i < mainCanvas.Children.Count; i++)
                {
                    var c = mainCanvas.Children[i];
                    Image img = c as Image;
                    if (img != null)
                    {
                        double iTop = (double)img.GetValue(Canvas.TopProperty);
                        double iLeft = (double)img.GetValue(Canvas.LeftProperty);

                        double iBottom = iTop + img.ActualHeight;
                        double iRight = iLeft + img.ActualWidth;

                        Rect iRect = new Rect(iLeft, iTop, iRight - iLeft, iBottom - iTop);
                        Point p = new Point(iLeft, iTop);

                        if (r.Contains(p))
                        {
                            double newPos = iTop - units;
                            if (newPos >= top)
                            {
                                img.SetValue(Canvas.TopProperty, newPos);
                            }
                            else
                            {
                                mainCanvas.Children.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            });
        }

        public void RemoveChars(int count)
        {
            invoke(() =>
            {
                if (count == 1 && inInputMode)
                {
                    if (_currentText.Length > 0)
                    {
                        char c = _currentText[_currentText.Length - 1];

                        double x = (double)_cursorCanvas.GetValue(Canvas.LeftProperty);
                        x -= GetStringWidth(c.ToString(), _currentInfo);
                        _cursorCanvas.SetValue(Canvas.LeftProperty, x);

                        _currentText.Remove(_currentText.Length - 1, 1);
                        removeLastChild();
                        if (_currentText.Length > 0)
                        {
                            SendStringToScreen(_currentText.ToString(), _currentInfo);
                        }
                        else
                        {
                            mainCanvas.Children.Add(new Image());
                        }
                    }
                }
                else
                {
                    removeLastChild();
                }

                lastDrawn = new Rect(_cursorX, _cursorY, 0, 0);
            });
        }

        public void GetColor(out int foreground, out int background)
        {
            foreground = _currentInfo.ForegroundColor;
            background = _currentInfo.BackgroundColor;
        }

        public void SetColor(int new_foreground, int new_background)
        {
            FlushCurrentString();

            long tempfg = FrotzNet.Frotz.Other.TrueColorStuff.GetColour(new_foreground);
            long tempbg = FrotzNet.Frotz.Other.TrueColorStuff.GetColour(new_background);

            _currentInfo.ForegroundColor = new_foreground;
            _currentInfo.BackgroundColor = new_background;
        }

        public ushort PeekColor()
        {
            return (ushort)_currentInfo.BackgroundColor;
        }

        public void SetInputMode(bool InputMode, bool CursorVisibility)
        {
            inInputMode = InputMode;
            if (inInputMode == false)
            {
                for (int i = 0; i < 10; i++)
                {
                    // TODO Move back to the carat
                }

                _scrollback.AddString(_currentText.ToString(), _currentInfo);

                _currentText.Clear();
            }
            else
            {
                if (_cursorX == lastDrawn.X)
                {
                    _cursorX += (int)lastDrawn.Width;
                }
            }

            invoke(() =>
            {
                mainCanvas.Children.Add(new Image());

                if (CursorVisibility)
                {
                    _cursorCanvas.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    _cursorCanvas.Visibility = System.Windows.Visibility.Hidden;
                }

                _cursorCanvas.SetValue(Canvas.TopProperty, (double)_cursorY + charHeight - _cursorCanvas.MinHeight);
                _cursorCanvas.SetValue(Canvas.LeftProperty, (double)_cursorX);
            });
        }

        public void SetInputColor()
        {
            _currentInfo.ForegroundColor = 32;
        }

        public void addInputChar(char c)
        {
            invoke(() =>
            {
                mainCanvas.Children.RemoveAt(mainCanvas.Children.Count - 1);

                _currentText.Append(c);

                SendStringToScreen(_currentText.ToString(), _currentInfo);

                double x = (double)_cursorCanvas.GetValue(Canvas.LeftProperty);
                x += GetStringWidth(c.ToString(), _currentInfo);
                _cursorCanvas.SetValue(Canvas.LeftProperty, x);
            });
        }

        public ZPoint GetCursorPosition()
        {
            return new ZPoint(_cursorX, _cursorY);
        }

        bool inInputMode = false;

        private void removeLastChild()
        {
            if (mainCanvas.Children.Count > 0)
            {
                mainCanvas.Children.RemoveAt(mainCanvas.Children.Count - 1);
            }
        }


        #region Copied from TextControlScreen

        int bColor; // Track the background color separately

        FrotzSound _sound;

        System.Windows.Controls.Canvas _cursorCanvas;

        int _cursorX = 0;
        int _cursorY = 0;


        public ScreenMetrics GetScreenMetrics()
        {
            invoke(() =>
            {
                SetCharsAndLines();

            });

            return _metrics;
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
                case ZFont.PICTURE_FONT:
                case ZFont.GRAPHICS_FONT:
                    return false;
            }

            return false;
        }

        public void StoryStarted(string StoryFileName, Blorb BlorbFile)
        {
            invoke(() =>
            {

                if (os_._blorbFile != null)
                {
                    _parent.Title = String.Format("FrotzNET - {0}", os_._blorbFile.StoryName);
                }
                else
                {
                    _parent.Title = String.Format("FrotzNET - {0}", StoryFileName);
                }

                OnStoryStarted(new GameSelectedEventArgs(StoryFileName, BlorbFile));
                _scrollback.Reset();

            });
        }

        public int GetStringWidth(string s, CharDisplayInfo Font)
        {
            int f = Font.Font;
            if (f == -1) f = _currentInfo.Font;

            FormattedText ft;
            if (f == ZFont.FIXED_WIDTH_FONT)
            {
                ft = buildFormattedText(s, _fixedFont, _currentInfo, null);
            }
            else if (f == ZFont.GRAPHICS_FONT)
            {
                ft = buildFormattedText(s, _fixedFont, _currentInfo, null);
            }
            else
            {
                ft = buildFormattedText(s, _regularFont, _currentInfo, null);
            }

            return (int)ft.WidthIncludingTrailingWhitespace;
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
                
                var fi = new System.IO.FileInfo(defaultName);
                ofd.FileName = fi.Name;
                
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

                var fi = new System.IO.FileInfo(defaultName);
                sfd.FileName = fi.Name;
                
                // sfd.FileName = defaultName;

                sfd.Title = Title;
                sfd.Filter = CreateFilterList(Filter);
                sfd.DefaultExt = DefaultExtension;

                if (sfd.ShowDialog(_parent) == true)
                {
                    name = sfd.FileName;
                }

                _parent.Focus(); // HACK For some reason, it won't always pick up text input after the dialog, so this refocuses
            }));
            return name;
        }

        private String CreateFilterList(params String[] types)
        {
            List<String> temp = new List<string>(types);
            temp.Add("All Files (*.*)|*.*");

            return String.Join("|", temp.ToArray());
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
            // TODO I don't know if this is ever hit?
            throw new NotImplementedException();
        }

        public void StopSample(int number)
        {
            invoke(() =>
            {
                _sound.StopSound();
            });
        }

        private static FrotzNetDLL.Frotz.Other.PNGChunk _palatteChunk = null;

        public void DrawPicture(int picture, byte[] Image, int y, int x)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                // If the image would go beyond the actual bounds of the display, don't bother drawing it.
                if (x > this.ActualWidth || y > this.ActualHeight) return;

                byte[] buffer = Image;

                if (os_._blorbFile.AdaptivePalatte != null && os_._blorbFile.AdaptivePalatte.Count > 0)
                {

                    try
                    {
                        // Had to use the adaptive palatte for some Infocom games
                        var ms = new System.IO.MemoryStream(Image);
                        FrotzNetDLL.Frotz.Other.PNG p = new FrotzNetDLL.Frotz.Other.PNG(ms);


                        if (os_._blorbFile.AdaptivePalatte.Contains(picture))
                        {
                            if (_palatteChunk == null) throw new ArgumentException("No last palette");
                            p.Chunks["PLTE"] = _palatteChunk;
                        }
                        else
                        {
                            _palatteChunk = p.Chunks["PLTE"];
                        }

                        ms = new System.IO.MemoryStream();
                        p.Save(ms);

                        buffer = ms.ToArray();
                    }
                    catch (ArgumentException)
                    {
                        // TODO It's bad form to not at least define the exception better
                    }
                }


                FrotzImage img = new FrotzImage();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new System.IO.MemoryStream(buffer);

                bi.EndInit();
                img.Source = bi;

                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                int newX = x;
                int newY = y;

                if (newY > short.MaxValue) newY -= ushort.MaxValue;
                if (newX > short.MaxValue) newX -= ushort.MaxValue;

                img.SetValue(Canvas.TopProperty, (double)newY);
                img.SetValue(Canvas.LeftProperty, (double)newX);

                img.SetValue(Canvas.HeightProperty, (double)(bi.Height * scale));
                img.SetValue(Canvas.WidthProperty, (double)(bi.Width * scale));

                if (picture == 1)
                {
                    if (img.Source.Width > mainCanvas.ActualWidth || img.Source.Height > mainCanvas.ActualHeight)
                    {
                        // Picture one is generallythe title page, Resize the img to be the same size as the canvas, 
                        // and it will show correctly in the bounds
                        img.SetValue(Canvas.WidthProperty, mainCanvas.ActualWidth);
                        img.SetValue(Canvas.HeightProperty, mainCanvas.ActualHeight);
                    }
                }

                mainCanvas.Children.Add(img);

                // removeCoveredImages(img);
            }));
        }

        private Rect GetImageBounds(Image img)
        {
            double x = (double)img.GetValue(Canvas.LeftProperty);
            double y = (double)img.GetValue(Canvas.TopProperty);
            double width = (double)img.GetValue(Canvas.WidthProperty);
            double height = (double)img.GetValue(Canvas.HeightProperty);

            if (double.IsNaN(width) && img.Source != null)
            {
                width = img.Source.Width;
                height = img.Source.Height;
            }

            return new Rect(x, y, width, height);
        }

        // Iterate through the images on the screen, and remove any that would be completely obscured by the new image
        // In additional to keeping the number of images on the screen down, this also allows text to be drawn on top
        // of other images (like Zork Zero status)
        private void removeCoveredImages(Image img)
        {
            Rect r = GetImageBounds(img);

            for (int i = 0; i < mainCanvas.Children.Count; i++)
            {
                Image oldImg = mainCanvas.Children[i] as Image;
                if (img == oldImg) continue;
                if (oldImg != null)
                {
                    Rect oldR = GetImageBounds(oldImg);

                    if (r.Contains(oldR) || r == oldR)
                    {
                        mainCanvas.Children.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        public string SelectGameFile(out byte[] filedata)
        {
            String fName = null;
            byte[] buffer = null;
            Dispatcher.Invoke(new Action(delegate
            {
                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.Title = "Open a Z-Code file";
                ofd.DefaultExt = ".dat";

                ofd.Filter = CreateFilterList(
                 "Most IF Files (*.zblorb;*.dat;*.z?;*.blorb)|*.zblorb;*.dat;*.z?;*.blorb",
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

        public void DisplayMessage(string Message, string Caption)
        {
            MessageBox.Show(Message, Caption);
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
        #endregion

        public new void Reset()
        {
            Clear();
        }

        protected override void AfterSetCharsAndLines()
        {
            _cursorCanvas.MinHeight = 2;
            _cursorCanvas.MinWidth = charWidth;
        }

        protected override void DoReset()
        {
            Clear();
        }

        public void SetActiveWindow(int win)
        {
            _activeWindow = win;
            FlushCurrentString();
        }

        public void SetWindowSize(int win, int top, int left, int height, int width)
        {
        }

        public bool ShouldWrap()
        {
            return true;
        }
    }
}

