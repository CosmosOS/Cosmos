using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace Cosmos.Build.Builder.Views
{
    public partial class MainWindow : UiWindow
    {
        public bool AppShutdown = true;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(this, Wpf.Ui.Appearance.BackgroundType.Mica, true);
            };
        }

        private void SectionTextCopyHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(((TextBlock)sender).Text);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (AppShutdown)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
