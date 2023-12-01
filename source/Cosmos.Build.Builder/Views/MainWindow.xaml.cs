using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Appearance;
using Clipboard = System.Windows.Clipboard;

namespace Cosmos.Build.Builder.Views
{
    public partial class MainWindow : Wpf.Ui.Controls.FluentWindow
    {
        public bool AppShutdown = true;

        public bool AllTasksCompleted;

        public bool ShowCloseBuilderDialog = true;

        public MainWindow()
        {
            InitializeComponent();
            SystemThemeWatcher.Watch(this);
        }

        private void SectionTextCopyHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(((TextBlock)sender).Text);
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!AllTasksCompleted && ShowCloseBuilderDialog)
            {
                e.Cancel = true;

                Wpf.Ui.Controls.MessageBox messageBox = new()
                {
                    Title = "Warning",
                    Content = $"You're about to close the builder, however tasks are still running.{Environment.NewLine}Do you wish to continue?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No"
                };
                if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    e.Cancel = false;
                }
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
