using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Clipboard = System.Windows.Clipboard;

namespace Cosmos.Build.Builder.Views
{
    public partial class MainWindow : UiWindow
    {
        public bool AppShutdown = true;

        public bool AllTasksCompleted;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) => Wpf.Ui.Appearance.Watcher.Watch(this);
        }

        private void SectionTextCopyHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(((TextBlock)sender).Text);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!AllTasksCompleted)
            {
                e.Cancel = true;

                Wpf.Ui.Controls.MessageBox messageBox = new()
                {
                    Content = "You're about to close the builder, however tasks are still running. Do you wish to continue?",
                    SizeToContent = SizeToContent.Width,
                    ButtonLeftAppearance = ControlAppearance.Secondary,
                    ButtonLeftName = "Yes",
                    ButtonRightAppearance = ControlAppearance.Primary,
                    ButtonRightName = "No"
                };
                messageBox.ButtonLeftClick += (_, _) =>
                {
                    messageBox.Close();
                    Application.Current.Dispatcher.Invoke(() => Application.Current?.MainWindow?.Close());
                };
                messageBox.ButtonRightClick += (_, _) => messageBox.Close();
                messageBox.ShowDialog();
            }
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
