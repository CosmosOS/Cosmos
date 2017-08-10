using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

using Frotz.Constants;
using Frotz.Screen;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace WPFMachine.Absolute
{
    public class ScrollbackArea
    {
        private DockPanel _dock;
        public DockPanel DP
        {
            get { return _dock; }
        }

        public RichTextBox _RTB;

        FlowDocument _doc;
        Paragraph _p;

        Run _currentRun = null;

        UserControl _parent;

        public ScrollbackArea(UserControl Parent)
        {
            _dock = new DockPanel();

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            _dock.Children.Add(sp);

            sp.SetValue(DockPanel.DockProperty, Dock.Top);

            Button bCopyText = new Button();
            bCopyText.Content = "Copy Text To Clipboard";
            bCopyText.Click += new RoutedEventHandler(bCopyText_Click);
            sp.Children.Add(bCopyText);

            Button bSaveRtf = new Button();
            bSaveRtf.Content = "Save RTF";
            bSaveRtf.Click += new RoutedEventHandler(bSaveRtf_Click);
            sp.Children.Add(bSaveRtf);

            Button bSaveText = new Button();
            bSaveText.Content = "Save Text";
            bSaveText.Click += new RoutedEventHandler(bSaveText_Click);
            sp.Children.Add(bSaveText);

            ScrollViewer sv = new ScrollViewer();
            _dock.Children.Add(sv);

            _RTB = new RichTextBox();
            _RTB.IsReadOnly = true;
            _RTB.IsReadOnlyCaretVisible = true;

            _doc = new FlowDocument();
            _RTB.Document = _doc;

            _parent = Parent;

            Reset();

            sv.Content =_RTB;
        }

        private void saveFile(String filter, String Format)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = filter;

            if (sfd.ShowDialog() == false)
            {
               return;
            }

            String fileName = sfd.FileName;

            var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create);

            var range = new TextRange(_RTB.Document.ContentStart, _RTB.Document.ContentEnd);
            range.Save(fs, Format, true);
            fs.Close();
        }

        void bSaveText_Click(object sender, RoutedEventArgs e)
        {
            saveFile("Text (*.txt)|*.txt", System.Windows.DataFormats.Text);
        }

        void bSaveRtf_Click(object sender, RoutedEventArgs e)
        {
            saveFile("Rich Text Format (*.rtf)|*.rtf", System.Windows.DataFormats.Rtf);
        }

        void bCopyText_Click(object sender, RoutedEventArgs e)
        {
            var range = new TextRange(_RTB.Document.ContentStart, _RTB.Document.ContentEnd);
            Clipboard.SetText(range.Text);
        }

        public void Reset()
        {
            _p = new Paragraph();
            _p.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.ProportionalFont);

            double PointSize = Properties.Settings.Default.FontSize  * (96.0 / 72.0);
            _p.FontSize = PointSize;

            _doc.Blocks.Clear();

            _doc.Blocks.Add(_p);

            _currentRun = null;

            currentStyle = -1;
        }

        String threeNewLines = "\r\n\r\n\r\n";
        int currentStyle = -1;

        public void AddString(String text, CharDisplayInfo cdi)
        {
            if (text == "") return;
            _parent.Dispatcher.Invoke(new Action(delegate
            {
               if (text == "\r\n") {

                  if (_p.Inlines.LastInline is LineBreak && _p.Inlines.LastInline.PreviousInline is LineBreak) {
                     return;
                  }
                  LineBreak lb = new LineBreak();
                  _p.Inlines.Add(lb);

                  _currentRun = null;
                  return;
               }


                if (currentStyle != cdi.Style)
                {
                    currentStyle = cdi.Style;
                    _currentRun = new Run();
                    _p.Inlines.Add(_currentRun);
                    if ((cdi.Style & ZStyles.BOLDFACE_STYLE) != 0)
                    {
                        _currentRun.FontWeight = FontWeights.Bold;
                    }
                    if ((cdi.Style & ZStyles.EMPHASIS_STYLE) != 0)
                    {
                        _currentRun.FontStyle = FontStyles.Italic;
                    }
                    if ( (cdi.Style & ZStyles.REVERSE_STYLE) != 0)
                    {
                        _currentRun.Background = Brushes.Black;
                        _currentRun.Foreground = Brushes.White;
                    }
                    if ( (cdi.Style & ZStyles.FIXED_WIDTH_STYLE) != 0)
                    {
                        _currentRun.FontFamily = new System.Windows.Media.FontFamily(Properties.Settings.Default.FixedWidthFont);
                    }
                }
                
                if (_currentRun == null)
                {
                    _currentRun = new Run(text);
                    _p.Inlines.Add(_currentRun);
                }
                else
                {
                    _currentRun.Text += text;

                    if (_currentRun.Text.EndsWith(threeNewLines))
                    {
                        StringBuilder sb = new StringBuilder(_currentRun.Text);

                        while (sb.ToString().EndsWith(threeNewLines))
                        {
                            sb.Remove(sb.Length - 2, 2);
                        }
                        _currentRun.Text = sb.ToString();

//                        
                    }
                }
                _RTB.CaretPosition = _RTB.CaretPosition.DocumentEnd;

            }));
        }
    }
}
