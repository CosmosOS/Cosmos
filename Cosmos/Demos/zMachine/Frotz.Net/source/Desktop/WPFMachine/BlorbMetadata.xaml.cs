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
using System.Windows.Shapes;

using System.Xml;
using System.IO;

namespace WPFMachine
{
    /// <summary>
    /// Interaction logic for BlorbMetadata.xaml
    /// </summary>
    public partial class BlorbMetadata : Window
    {
        private Frotz.Blorb.Blorb _blorb;

        public BlorbMetadata(Frotz.Blorb.Blorb BlorbFile)
        {
            InitializeComponent();

            rtb.SizeChanged += new SizeChangedEventHandler(rtb_SizeChanged);
            imgCover.SizeChanged += new SizeChangedEventHandler(imgCover_SizeChanged);

            _blorb = BlorbFile;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(_blorb.MetaData);

            int row = 0;

            XmlNodeList nodes;

            if (BlorbFile.Pictures.Count > 0)
            {
                nodes = xml.GetElementsByTagName("coverpicture");
                if (nodes.Count > 0)
                {
                    int id = Convert.ToInt32(nodes[0].InnerText);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(BlorbFile.Pictures[id].Image);
                    bi.EndInit();
                    imgCover.Source = bi;
                }
                
            }




            nodes = xml.GetElementsByTagName("bibliographic");
            if (nodes.Count == 1)
            {
                foreach (XmlNode node in nodes[0].ChildNodes)
                {
                    if (node.Name == "description")
                    {
                        wbInfo.NavigateToString(node.InnerXml);
                    }
                    else
                    {
                        String text = "";
                        String key = node.Name;
                        switch (key)
                        {
                            case "title":
                                {
                                    text = "Title";
                                    this.Title = node.InnerText;
                                }
                                break;
                            case "author":
                                text = "Author"; break;
                            case "language":
                                text = "Language"; break;
                            case "headline":
                                text = "Subtitle"; break;
                            case "firstpublished":
                                text = "First Published"; break;
                            case "genre":
                                text = "Genre"; break;
                            case "group":
                                text = "Group"; break;
                            case "series":
                                text = "Series"; break;
                            case "seriesnumber":
                                text = "Series #"; break;
                        }

                        if (text == "Language") continue; // Temporary measure, since I don't want to see the language

                        TableRow tr = new TableRow();
                        TableCell tc = new TableCell(new Paragraph(new Run(text)));
                        tr.Cells.Add(tc);

                        Paragraph p = new Paragraph();
                        Run r = new Run(node.InnerText);
                        p.TextAlignment = TextAlignment.Right;
                        p.Inlines.Add(r);

                        tc = new TableCell(p);
                        tr.Cells.Add(tc);

                        trg.Rows.Add(tr);

                        row++;
                    }
                }

                btnOk.Focus();
            }

            nodes = xml.GetElementsByTagName("contacts");
            if (nodes.Count > 0)
            {
                var n = nodes[0];
                if (n.FirstChild.Name == "url")
                {

                    TableRow tr = new TableRow();
                    TableCell tc = new TableCell(new Paragraph(new Run("More Info")));
                    tr.Cells.Add(tc);

                    Paragraph p = new Paragraph();
                    Hyperlink h = new Hyperlink(new Run(n.FirstChild.InnerText));
                    h.Focusable = false;
                    h.Foreground = Brushes.Blue;
                    h.IsEnabled = true;
                    p.TextAlignment = TextAlignment.Right;
                    h.MouseDown += new MouseButtonEventHandler(h_MouseDown);
                    h.NavigateUri = new Uri(n.FirstChild.InnerText);
                    h.ForceCursor = true;
                    h.Cursor = Cursors.Hand;
                    p.Inlines.Add(h);

                    tc = new TableCell(p);
                    tr.Cells.Add(tc);
                    trg.Rows.Add(tr);
                }
            }
        }

        void h_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Hyperlink hl = sender as Hyperlink;
            wbInfo.Navigate(hl.NavigateUri.ToString(), "_blank", null, null);
        }

        void imgCover_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            checkSize();
        }

        void rtb_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            checkSize();
        }
        

        bool resized = false;
        private void checkSize()
        {
            if (resized == false && rtb.ActualHeight > 0 && imgCover.ActualHeight > 0)
            {
                this.Height = rtb.ActualHeight + imgCover.ActualHeight + 50 + (this.ActualHeight - LayoutRoot.ActualHeight);

                resized = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
