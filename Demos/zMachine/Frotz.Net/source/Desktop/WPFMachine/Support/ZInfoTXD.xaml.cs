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

using System.Text.RegularExpressions;

namespace WPFMachine.Support
{
    /// <summary>
    /// Interaction logic for ZInfoTXD.xaml
    /// </summary>
    public partial class ZInfoTXD : UserControl
    {
        String _text;

        public ZInfoTXD(String text, int type)
        {
            InitializeComponent();

            tb.FontFamily = new FontFamily("Consolas");
            parse(text, type);

            _text = text;
        }

        private void parse(String text, int type)
        {
            Regex rgx = new Regex(@"^(S\d+:)(.*)");

            Run r;

            StringBuilder sb = new StringBuilder();
            var sr = new System.IO.StringReader(text);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                if (type == 0)
                {
                    if (line.StartsWith("Routine"))
                    {
                        r = new Run(sb.ToString());
                        tb.Inlines.Add(r);

                        r = new Run(line + "\n");
                        r.Foreground = Brushes.Red;
                        r.FontWeight = FontWeights.Bold;
                        tb.Inlines.Add(r);

                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(line + "\n");
                    }
                }
                else
                {
                    Match m = rgx.Match(line);
                    if (m.Success)
                    {
                        r = new Run(sb.ToString());
                        tb.Inlines.Add(r);

                        r = new Run(m.Groups[1].Value);
                        r.Foreground = Brushes.Red;
                        r.FontWeight = FontWeights.Bold;
                        tb.Inlines.Add(r);
                        r = new Run(m.Groups[2].Value + "\n");
                        tb.Inlines.Add(r);

                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(line + "\n");
                    }
                }
            }

            r = new Run(sb.ToString());
            tb.Inlines.Add(r);
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_text);
        }
#if TEMP
        private void parseCode(String text)
        {
            FlowDocument fd = new FlowDocument();

            rtb.Document = fd;

            Run r;
            Paragraph p;

            StringBuilder sb = new StringBuilder();
            var sr = new System.IO.StringReader(text);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("Routine"))
                {
                    r = new Run(sb.ToString());
                    p = new Paragraph(r);
                    p.Margin = new Thickness(0);
                    p.Padding = new Thickness(0);
                    fd.Blocks.Add(p);

                    r = new Run(line + "\n");
                    r.Foreground = Brushes.Red;
                    r.FontWeight = FontWeights.Bold;
                    p = new Paragraph(r);
                    p.Margin = new Thickness(0);
                    p.Padding = new Thickness(0);
                    fd.Blocks.Add(p);

                    sb.Clear();
                }
                else
                {
                    sb.Append(line + "\n");
                }
            }

            r = new Run(sb.ToString());
            p = new Paragraph(r);
            fd.Blocks.Add(p);
        }

        private void parseStrings(String text)
        {
            Regex rgx = new Regex(@"^(S\d+:)(.*)");


            FlowDocument fd = new FlowDocument();
            fd.LineStackingStrategy = LineStackingStrategy.MaxHeight;

            rtb.Document = fd;

            Run r;
            Paragraph p;

            StringBuilder sb = new StringBuilder();
            var sr = new System.IO.StringReader(text);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
            
            }

            r = new Run(sb.ToString());
            p = new Paragraph(r);
            fd.Blocks.Add(p);
        }
#endif
    }
}
