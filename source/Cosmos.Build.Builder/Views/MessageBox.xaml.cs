﻿using System.Windows;
using System.Windows.Documents;
using Wpf.Ui.Controls;
using Hyperlink = System.Windows.Documents.Hyperlink;

namespace Cosmos.Build.Builder.Views
{
    public partial class MessageBox : Window
    {
        public MessageBox(string Content)
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
            };

            if (Content.StartsWith("link:"))
            {
                Content = Content.Replace("link:", "");
                var hlink = new Hyperlink();
                hlink.Inlines.Add(Content);
                hlink.NavigateUri = new System.Uri(Content);
                hlink.RequestNavigate += (sender, e) =>
                {
                    System.Diagnostics.Process.Start(e.Uri.ToString());
                };
                lblMain.Text = "";
                lblMain.Inlines.Add("Click me: ");
                lblMain.Inlines.Add(hlink);
            }
            else
            {
                lblMain.Text = Content;
            }

        }

        public static void Show(string Content)
        {
            var window = new MessageBox(Content);

            //this workarounds a bug that when the main window is minimized then brought to front, the message box is no longer visible
            window.Topmost = true;
            window.ShowDialog();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Topmost = false;
            Close();
        }
    }
}
